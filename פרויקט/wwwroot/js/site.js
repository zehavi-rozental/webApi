const uri = '/api/IceCream';
const userApiUri = '/api/User';
let iceCreams = [];
let signalRConnection = null;

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

function base64UrlDecode(str) {
    str = str.replace(/-/g, '+').replace(/_/g, '/');
    while (str.length % 4) {
        str += '=';
    }
    const bytes = Uint8Array.from(atob(str), c => c.charCodeAt(0));
    if (typeof TextDecoder !== 'undefined') {
        return new TextDecoder('utf-8').decode(bytes);
    }
    return String.fromCharCode.apply(null, bytes);
}

function getPayloadFromToken() {
    const token = getToken();
    if (!token) return null;
    const parts = token.split('.');
    if (parts.length < 2) return null;
    try {
        const json = base64UrlDecode(parts[1]);
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
        showToast('Item not found', 'error');
        return;
    }
    document.getElementById('edit-name').value = item.name;
    document.getElementById('edit-id').value = item.id;
    document.getElementById('edit-milki').checked = item.Milki || item.milki || false;
    document.getElementById('editForm').style.display = 'block';
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
    document.getElementById('editForm').style.display = 'none';
}

// ===== Display Functions =====
function _displayCount(itemCount) {
    const name = (itemCount === 1) ? 'iceCream' : 'Types of ice cream';
    document.getElementById('counter').innerText = `${itemCount} ${name}`;
}

function _displayItems(data) {
    iceCreams = Array.isArray(data) ? data : [];
    const tBody = document.getElementById('iceCreams');
    tBody.innerHTML = '';

    _displayCount(iceCreams.length);

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
    signalRConnection = new signalR.HubConnectionBuilder()
        .withUrl("/activityHub", {
            accessTokenFactory: () => getToken()
        })
        .withAutomaticReconnect()
        .build();

    // Receive activity notifications and update grid
    signalRConnection.on("ReceiveActivity", function (data) {
        if (!data) return;
        
        // Show toast notification for the activity
        const username = data.username || 'Someone';
        const action = data.action || 'performed action';
        const itemName = data.itemName || 'item';
        
        let message = '';
        switch (action) {
            case 'added':
                message = `Ice cream '${itemName}' was added`;
                break;
            case 'updated':
                message = `Ice cream '${itemName}' was updated`;
                break;
            case 'deleted':
                message = `Ice cream '${itemName}' was deleted`;
                break;
            default:
                message = `${username} ${action} '${itemName}'`;
        }
        
        showToast(message, 'info');
        
        // Update the grid based on the changed item (avoid full refresh when possible)
        const updatedItem = data.item;
        if (updatedItem && typeof updatedItem.id !== 'undefined') {
            switch (action) {
                case 'added':
                    iceCreams.push(updatedItem);
                    break;
                case 'updated':
                    const idx = iceCreams.findIndex(i => i.id === updatedItem.id);
                    if (idx >= 0) {
                        iceCreams[idx] = updatedItem;
                    } else {
                        iceCreams.push(updatedItem);
                    }
                    break;
                case 'deleted':
                    iceCreams = iceCreams.filter(i => i.id !== updatedItem.id);
                    break;
                default:
                    break;
            }
            _displayItems(iceCreams);
        } else {
            // Fallback: refresh the list if we don't have item details
            getItems();
        }
    });

    signalRConnection.on("UserConnected", function (data) {
        console.log("Ucted:", data);
    });

    signalRConnection.on("UserDisconnected", function (data) {
        console.log("User disconnected:", data);
    });

    signalRConnection.start()
        .then(() => {
            console.log("Scted");
            showToast('Connected to real-time updates', 'success');
        })
        .catch(err => {
            console.error("SignalR connection error:", err);
            showToast('Failed to connect to real-time updates', 'warning');
        });
}

// ===== Toggle Profile Form =====
function toggleProfileForm() {
    const form = document.getElementById('profileForm');
    form.style.display = form.style.display === 'none' || form.style.display === '' ? 'block' : 'none';
}

// ===== Update Profile =====
function updateProfile() {
    const name = document.getElementById('profile-name').value.trim();
    const password = document.getElementById('profile-password').value;

    if (!name) {
        showToast('Name cannot be empty', 'warning');
        return;
    }

    const payload = { name };
    if (password) {
        payload.password = password;
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
                showToast('Profile updated successfully!', 'success');
            } else {
                return Promise.reject('Update failed');
            }
        })
        .catch(error => {
            console.error('Unable to update profile.', error);
            showToast('Failed to update profile', 'error');
        });
}

// ===== Logout =====
function handleLogout() {
    removeToken();
    if (signalRConnection) {
        signalRConnection.stop();
    }
    location.href = 'login.html';
}
