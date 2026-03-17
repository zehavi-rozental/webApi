const userUri = '/api/User';
let allUsers = [];

// authFetch is provided by site.js

function getItems() {
    authFetch(userUri)
        .then(response => response.json())
        .then(data => _displayItems(data))
        .catch(error => console.error('Unable to get items.', error));
}

function addItem() {
    const addNameTextbox = document.getElementById('add-name');
    const addPasswordTextbox = document.getElementById('add-password');
    const addRoleSelect = document.getElementById('add-role');
    
    const name = addNameTextbox.value.trim();
    const password = addPasswordTextbox.value.trim();
    const role = addRoleSelect.value.trim();

    // Validate inputs
    if (!name) {
        showToast('Please enter a user name', 'warning');
        addNameTextbox.focus();
        return false;
    }
    
    if (!password) {
        showToast('Please enter a password', 'warning');
        addPasswordTextbox.focus();
        return false;
    }

    if (password.length < 3) {
        showToast('Password must be at least 3 characters', 'warning');
        return false;
    }

    const item = {
        Name: name,
        Password: password,
        Role: role || 'User'
    };

    authFetch(userUri, {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(item)
    })
        .then(response => {
            if (response.ok) {
                showToast('User added successfully!', 'success');
                addNameTextbox.value = '';
                addPasswordTextbox.value = '';
                addRoleSelect.value = 'User';
                getItems();
            } else if (response.status === 400) {
                return response.json().then(err => {
                    throw new Error(err.message || 'Invalid input');
                });
            } else {
                throw new Error('Failed to add user');
            }
        })
        .catch(error => {
            console.error('Unable to add user:', error);
            showToast('Failed to add user: ' + error.message, 'error');
        });
    return false;
}

function deleteItem(id) {
    if (!confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
        return false;
    }

    authFetch(`${userUri}/${id}`, {
        method: 'DELETE'
    })
        .then(response => {
            if (response.ok) {
                showToast('User deleted successfully!', 'success');
                getItems();
            } else if (response.status === 403) {
                showToast('You cannot delete your own account or lack permission', 'error');
            } else if (response.status === 404) {
                showToast('User not found', 'error');
            } else {
                throw new Error('Delete failed');
            }
        })
        .catch(error => {
            console.error('Unable to delete user:', error);
            showToast('Failed to delete user', 'error');
        });
}

function displayEditForm(id) {
    const item = allUsers.find(item => {
        return (item.Id === id || item.id === id);
    });

    if (!item) {
        showToast('User not found', 'error');
        return false;
    }

    // Populate form with user data
    document.getElementById('edit-name').value = item.Name || item.name || '';
    document.getElementById('edit-password').value = ''; // Always clear for security
    document.getElementById('edit-role').value = item.Role || item.role || 'User';
    document.getElementById('edit-id').value = item.Id || item.id;

    // Show edit form with glassmorphic animation
    const form = document.getElementById('editForm');
    form.classList.remove('hidden');
    form.classList.add('show');
    document.getElementById('edit-name').focus();

    return false;
}

function updateItem() {
    const itemId = document.getElementById('edit-id').value;
    const name = document.getElementById('edit-name').value.trim();
    const password = document.getElementById('edit-password').value.trim();
    const role = document.getElementById('edit-role').value.trim();

    // Validation
    if (!itemId) {
        showToast('No user selected for update', 'error');
        return false;
    }

    if (!name) {
        showToast('Name cannot be empty', 'warning');
        return false;
    }

    // Find original user to verify it exists
    const original = allUsers.find(item => (item.Id == itemId || item.id == itemId));
    if (!original) {
        showToast('User not found', 'error');
        return false;
    }

    // Build update payload
    const item = {
        Id: parseInt(itemId, 10),
        Name: name,
        Role: role || 'User'
    };
    
    // Only include password if provided
    if (password) {
        if (password.length < 3) {
            showToast('Password must be at least 3 characters', 'warning');
            return false;
        }
        item.Password = password;
    }

    authFetch(`${userUri}/${itemId}`, {
        method: 'PUT',
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(item)
    })
        .then(response => {
            if (response.ok) {
                showToast('User updated successfully!', 'success');
                closeInput();
                getItems();
            } else if (response.status === 403) {
                showToast('You do not have permission to edit this user', 'error');
            } else if (response.status === 404) {
                showToast('User not found', 'error');
            } else {
                throw new Error('Update failed');
            }
        })
        .catch(error => {
            console.error('Unable to update user:', error);
            showToast('Failed to update user', 'error');
        });

    return false;
}

function closeInput() {
    const form = document.getElementById('editForm');
    form.classList.remove('show');
    form.classList.add('hidden');
    
    // Clear form fields securely
    document.getElementById('edit-name').value = '';
    document.getElementById('edit-password').value = '';
    document.getElementById('edit-role').value = 'User';
    document.getElementById('edit-id').value = '';
}

function _displayCount(itemCount) {
    const name = (itemCount === 1) ? 'user' : 'users';

    document.getElementById('counter').innerText = `${itemCount} ${name}`;
}

function _displayItems(data) {
    const tBody = document.getElementById('users');
    tBody.innerHTML = '';

    _displayCount(data.length);

    const button = document.createElement('button');

    data.forEach(item => {
        let editButton = button.cloneNode(false);
        editButton.innerText = 'Edit';
        const itemId = item.Id || item.id;
        editButton.setAttribute('onclick', `displayEditForm(${itemId})`);

        let deleteButton = button.cloneNode(false);
        deleteButton.innerText = 'Delete';
        deleteButton.setAttribute('onclick', `deleteItem(${itemId})`);

        let tr = tBody.insertRow();

        let td1 = tr.insertCell(0);
        let textNode = document.createTextNode(item.Name || item.name || '');
        td1.appendChild(textNode);

        let td2 = tr.insertCell(1);
        td2.appendChild(editButton);

        let td3 = tr.insertCell(2);
        td3.appendChild(deleteButton);
    });

    allUsers = data;
}