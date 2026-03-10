const uri = '/api/User';
let users = [];

function authFetch(url, opts = {}) {
    const token = (typeof getToken === 'function' ? getToken() : localStorage.getItem('token'));
    opts.headers = Object.assign({}, opts.headers || {}, token ? { 'Authorization': 'Bearer ' + token } : {});
    return fetch(url, opts).then(response => {
        if (response.status === 401) {
           
            localStorage.removeItem('token');
            if (window.showInlineLogin) {
                try { window.showInlineLogin(); } catch (e) { }
            } else {
                location.href = 'login.html';
            }
            throw new Error('Unauthorized');
        }
        return response;
    });
}

function getItems() {
    authFetch(uri)
        .then(response => response.json())
        .then(data => _displayItems(data))
        .catch(error => console.error('Unable to get items.', error));
}

function addItem() {
    const addNameTextbox = document.getElementById('add-name');
    const item = {
        Name: addNameTextbox.value.trim()
    };

    if (!item.Name) {
        showToast('Please enter a user name', 'warning');
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
                showToast('User added successfully!', 'success');
                addNameTextbox.value = '';
                getItems();
            } else {
                return Promise.reject('Add failed');
            }
        })
        .catch(error => {
            console.error('Unable to add item.', error);
            showToast('Failed to add user', 'error');
        });
    return false;
}

function deleteItem(id) {
    authFetch(`${uri}/${id}`, {
            method: 'DELETE'
        })
        .then(() => getItems())
        .catch(error => console.error('Unable to delete item.', error));
}

function displayEditForm(id) {
    const item = users.find(item => item.Id === id || item.id === id);

    if (!item) {
        showToast('Item not found', 'error');
        return;
    }

    document.getElementById('edit-name').value = item.Name || item.name || '';
    document.getElementById('edit-id').value = item.Id || item.id;
    document.getElementById('editForm').style.display = 'block';
}

function updateItem() {
    const itemId = document.getElementById('edit-id').value;
    const item = {
        Id: parseInt(itemId, 10),
        Name: document.getElementById('edit-name').value.trim()
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
                showToast('User updated successfully!', 'success');
                closeInput();
                getItems();
            } else {
                return Promise.reject('Update failed');
            }
        })
        .catch(error => {
            console.error('Unable to update item.', error);
            showToast('Failed to update user', 'error');
        });

    return false;
}

function closeInput() {
    document.getElementById('editForm').style.display = 'none';
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

    users = data;
}