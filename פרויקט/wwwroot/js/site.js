// ===== API CONFIGURATION =====
const uri = '/api/IceCream';
const userApiUri = '/api/User';

// ===== STATE MANAGEMENT =====
let iceCreams = [];
let signalRConnection = null;
let isConnected = false;

// ===== Toast Notification System =====
function showToast(message, type = 'info', duration = 3000) {
    const container = document.getElementById('toastContainer') || createToastContainer();
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.textContent = message;
    toast.setAttribute('role', 'status');
    container.appendChild(toast);
    
    setTimeout(() => {
        toast.classList.add('show');
    }, 10);
    
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => container.removeChild(toast), 300);
    }, duration);
}

function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toastContainer';
    container.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        display: flex;
        flex-direction: column;
        gap: 10px;
    `;
    document.body.appendChild(container);
    return container;
}

// ===== Token and Authorization =====
function getToken() { 
    return localStorage.getItem('token'); 
}

function setToken(token) {  
    localStorage.setItem('token', token); 
}

function removeToken() { 
    localStorage.removeItem('token'); 
}

function getPayloadFromToken() {
    const token = getToken();
    if (!token) return null;
    const parts = token.split('.');
    if (parts.length < 2) return null;
    try {
        const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
        const json = atob(base64);
        return JSON.parse(json);
    } catch (e) {
        return null;
    }
}

function getUserName() {
    const p = getPayloadFromToken();
    if (!p) return null;
    return p.name || p.unique_name || p.username || p.sub || null;
}

function getUserRole() {
    const p = getPayloadFromToken();
    if (!p) return null;
    return p.role || p['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
}

function getUserId() {
    const p = getPayloadFromToken();
    return p?.userId || p?.sub;
}

// ===== Authenticated Fetch =====
function authFetch(url, opts = {}) {
    const token = localStorage.getItem('token'); 
    opts.headers = Object.assign({}, opts.headers || {}, token ? { 'Authorization': 'Bearer ' + token } : {});
    return fetch(url, opts).then(response => {
        if (response.status === 401) {
            localStorage.removeItem('token');
            if (window.showInlineLogin) {
                try { window.showInlineLogin(); } catch (e) { }
            } else {
                // fallback: redirect to login page
                location.href = 'login.html';
            }
            throw new Error('Unauthorized');
        }
        return response;
    });
}

// ===== CRUD Operations =====
function getItems() {
    authFetch(uri)
        .then(response => response.json())
        .then(data => _displayItems(data))
        .catch(error => {
            console.error('Unable to get items.', error);
            showToast('Failed to load items', 'error');
        });
}

function addItem() {
    const addNameTextbox = document.getElementById('add-name');
    const item = {
        Name: addNameTextbox.value.trim(),
        Milki: false
    };

    if (!item.Name) {
        showToast('Please enter an ice cream name', 'warning');
        return false;
    }

    authFetch(uri, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(item)
    })
        .then(response => {
            if (response.ok) {
                addNameTextbox.value = '';
                // Grid will update when SignalR notifies
            } else {
                return response.status === 401 ? Promise.reject('Unauthorized') : Promise.reject('Add failed');
            }
        })
        .catch(error => {
            console.error('Unable to add item.', error);
            showToast('Failed to add ice cream', 'error');
        });
    return false;
}

function deleteItem(id) {
    if (!confirm('Are you sure you want to delete this item?')) return;
    
    authFetch(`${uri}/${id}`, {
        method: 'DELETE'
    })
        .then(response => {
            if (response.ok) {
                // Grid will update when SignalR notifies
            } else {
                return Promise.reject('Delete failed');
            }
        })
        .catch(error => {
            console.error('Unable to delete item.', error);
            showToast('Failed to delete ice cream', 'error');
        });
}

function displayEditIceCreamForm(id) {
    const item = iceCreams.find(item => item.id === id);
    if (!item) {
        showToast('Ice cream not found', 'error');
        return false;
    }

    // Populate form fields
    document.getElementById('edit-name').value = item.name || '';
    document.getElementById('edit-id').value = item.id || '';
    document.getElementById('edit-milki').checked = item.Milki || item.milki || false;

    // Show form with glassmorphic animation
    const form = document.getElementById('editForm');
    form.classList.remove('hidden');
    form.classList.add('show');
    document.getElementById('edit-name').focus();

    return false;
}

function updateItem() {
    const itemId = document.getElementById('edit-id').value;
    const name = document.getElementById('edit-name').value.trim();
    const milki = document.getElementById('edit-milki').checked;

    if (!itemId) {
        showToast('No item selected for update', 'error');
        return false;
    }
    if (!name) {
        showToast('Name cannot be empty', 'warning');
        return false;
    }

    // Find the original item
    const original = iceCreams.find(item => item.id == itemId);
    if (!original) {
        showToast('Original item not found', 'error');
        return false;
    }

    // Only send changed fields (defensive, but not required)
    const item = {
        Id: parseInt(itemId, 10),
        Name: name,
        Milki: milki
    };

    authFetch(`${uri}/${itemId}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(item)
    })
        .then(response => {
            if (response.ok) {
                closeInput();
            } else if (response.status === 403) {
                showToast('You are not allowed to edit this item', 'error');
            } else if (response.status === 404) {
                showToast('Item not found', 'error');
            } else {
                showToast('Failed to update ice cream', 'error');
            }
        })
        .catch(error => {
            console.error('Unable to update item.', error);
            showToast('Failed to update ice cream', 'error');
        });
    return false;
}

function closeInput() {
    const form = document.getElementById('editForm');
    form.classList.remove('show');
    form.classList.add('hidden');
    
    // Clear form fields
    document.getElementById('edit-name').value = '';
    document.getElementById('edit-id').value = '';
    document.getElementById('edit-milki').checked = false;
}

// ===== Display Functions =====
function _displayCount(itemCount) {
    const name = (itemCount === 1) ? 'iceCream' : 'Types of ice cream';
    document.getElementById('counter').innerText = `${itemCount} ${name}`;
}

function _displayItems(data) {
    const tBody = document.getElementById('iceCreams');
    tBody.innerHTML = '';

    _displayCount(data.length);

    const button = document.createElement('button');

    data.forEach(item => {
        let MilkiCheckbox = document.createElement('input');
        MilkiCheckbox.type = 'checkbox';
        MilkiCheckbox.disabled = true;
        MilkiCheckbox.checked = item.Milki ?? item.milki ?? false;

        let editButton = button.cloneNode(false);
        editButton.innerText = 'Edit';
        editButton.setAttribute('onclick', `displayEditIceCreamForm(${item.id})`);

        let deleteButton = button.cloneNode(false);
        deleteButton.innerText = 'Delete';
        deleteButton.setAttribute('onclick', `deleteItem(${item.id})`);

        let tr = tBody.insertRow();

        let td1 = tr.insertCell(0);
        td1.appendChild(MilkiCheckbox);

        let td2 = tr.insertCell(1);
        let textNode = document.createTextNode(item.name);
        td2.appendChild(textNode);

        let td3 = tr.insertCell(2);
        td3.appendChild(editButton);

        let td4 = tr.insertCell(3);
        td4.appendChild(deleteButton);
    });

    iceCreams = data;
}

// ===== SignalR Integration =====
function initSignalR() {
    if (!window.signalR) {
        console.warn('SignalR not loaded');
        return;
    }

    signalRConnection = new signalR.HubConnectionBuilder()
        .withUrl("/activityHub", {
            accessTokenFactory: () => getToken()
        })
        .withAutomaticReconnect({
            nextRetryDelayInMilliseconds: retryContext => {
                if (retryContext.previousRetryCount === 0) { return 0; }
                if (retryContext.previousRetryCount === 1) { return 2000; }
                if (retryContext.previousRetryCount < 5) { return 5000; }
                return 10000;
            }
        })
        .build();

    // Handle connection state
    signalRConnection.onreconnected(() => {
        isConnected = true;
        showToast('Reconnected to real-time updates', 'success');
        getItems(); // Refresh data on reconnect
    });

    signalRConnection.onreconnecting(() => {
        isConnected = false;
        showToast('Attempting to reconnect...', 'warning');
    });

    signalRConnection.onclose(() => {
        isConnected = false;
        showToast('Connection lost. Will attempt to reconnect...', 'warning');
    });

    // Receive activity notifications and update grid
    signalRConnection.on("ReceiveActivity", function (data) {
        if (!data) return;
        
        const username = data.username || 'Someone';
        const action = data.action || 'performed action';
        const itemName = data.itemName || 'item';
        
        let message = '';
        switch (action) {
            case 'added':
                message = `'${itemName}' was added`;
                break;
            case 'updated':
                message = `'${itemName}' was updated`;
                break;
            case 'deleted':
                message = `'${itemName}' was deleted`;
                break;
            default:
                message = `${username} ${action}`;
        }
        
        showToast(message, 'info');
        getItems(); // Refresh the grid
    });

    signalRConnection.start()
        .then(() => {
            isConnected = true;
            console.log('Connected to activity hub');
            showToast('Connected to real-time updates', 'success');
        })
        .catch(err => {
            isConnected = false;
            console.error("SignalR connection error:", err);
            showToast('Real-time updates unavailable', 'warning');
        });
}

// ===== Toggle Profile Form =====
function toggleProfileForm() {
    const form = document.getElementById('profileForm');
    if (!form) return;
    
    const isHidden = form.classList.contains('hidden');
    if (isHidden) {
        // Opening: Pre-fill current name from greeting
        const currentName = getUserName();
        if (currentName) {
            document.getElementById('profile-name').value = currentName;
        }
        // Clear password field for security
        document.getElementById('profile-password').value = '';
        
        form.classList.remove('hidden');
        form.classList.add('show');
        document.getElementById('profile-name').focus();
    } else {
        // Closing: Clear form
        form.classList.remove('show');
        form.classList.add('hidden');
        document.getElementById('profile-name').value = '';
        document.getElementById('profile-password').value = '';
    }
}

// ===== Update Profile =====
function updateProfile() {
    const name = document.getElementById('profile-name').value.trim();
    const password = document.getElementById('profile-password').value.trim();

    if (!name) {
        showToast('Name cannot be empty', 'warning');
        document.getElementById('profile-name').focus();
        return false;
    }

    // Get current user ID from token
    const payload = {
        Name: name,
        // SECURITY: Only send Name and Password, NEVER include Role to prevent privilege escalation
    };
    
    if (password) {
        if (password.length < 3) {
            showToast('Password must be at least 3 characters', 'warning');
            return false;
        }
        payload.Password = password;
    }

    authFetch(`${userApiUri}/profile`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
    })
        .then(response => {
            if (response.ok) {
                showToast('Profile updated successfully! ✅', 'success');
                
                // Update greeting in navbar without page refresh
                updateGreeting(name);
                
                // Close profile form
                toggleProfileForm();
                
                // Clear password field
                document.getElementById('profile-password').value = '';
            } else if (response.status === 400) {
                return response.json().then(err => {
                    throw new Error(err.message || 'Invalid input');
                });
            } else if (response.status === 401) {
                showToast('Session expired. Please log in again.', 'error');
                handleLogout();
            } else {
                throw new Error('Update failed');
            }
        })
        .catch(error => {
            console.error('Unable to update profile:', error);
            showToast('Failed to update profile: ' + error.message, 'error');
        });
    return false;
}

// ===== Update Greeting in Navbar =====
function updateGreeting(newName) {
    const greetingEl = document.getElementById('greeting');
    if (greetingEl) {
        greetingEl.textContent = `👋 Welcome, ${newName}!`;
    }
}

// ===== Logout =====
function handleLogout() {
    removeToken();
    if (signalRConnection) {
        signalRConnection.stop();
    }
    location.href = 'login.html';
}
