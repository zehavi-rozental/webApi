const uri = '/api/IceCream';
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

function isAdmin() {
    const role = getUserRole();
    return role && (role === 'Admin' || role.toLowerCase() === 'admin');
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
                showToast('Ice cream added! Waiting for confirmation...', 'success');
                addNameTextbox.value = '';
                getItems();
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
                showToast('Ice cream deleted! Waiting for confirmation...', 'success');
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

function displayEditForm(id) {
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
    const item = {
        Id: parseInt(itemId, 10),
        Name: document.getElementById('edit-name').value.trim(),
        Milki: document.getElementById('edit-milki').checked
    };

    if (!item.Name) {
        showToast('Name cannot be empty', 'warning');
        return false;
    }

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
                showToast('Ice cream updated! Waiting for confirmation...', 'success');
                closeInput();
                getItems();
            } else {
                return Promise.reject('Update failed');
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
        editButton.setAttribute('onclick', `displayEditForm(${item.id})`);

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
        .withUrl("/activityHub")
        .withAutomaticReconnect()
        .build();

    // Receive activity notifications
    signalRConnection.on("ReceiveActivity", function (data) {
        if (!data) return;
        
        const username = data.username || data; // Handle both object and string formats
        const action = data.action || 'activity';
        const itemName = data.itemName || '';
        
        const activityList = document.getElementById("activityList");
        if (activityList) {
            const li = document.createElement("li");
            li.textContent = `${username} ${action} '${itemName}'`;
            activityList.insertBefore(li, activityList.firstChild);

            // Keep list short
            while (activityList.children.length > 10) {
                activityList.removeChild(activityList.lastChild);
            }
        }

        // Refresh the grid when activity is received
        getItems();
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

// ===== Logout =====
function handleLogout() {
    removeToken();
    if (signalRConnection) {
        signalRConnection.stop();
    }
    // Show login form instead of redirecting to login.html
    location.reload();
}
