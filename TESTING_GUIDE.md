# 🧪 TESTING GUIDE - SCOOPY PROFESSIONAL REFACTOR

## Quick Start Testing

### Prerequisites
1. ✅ .NET 9.0 Web API running on `http://localhost:5000`
2. ✅ Browser with developer tools (F12)
3. ✅ Test user account (or create one)
4. ✅ Cache cleared (Ctrl+Shift+Delete)

---

## 🔐 Authentication Flow

### Test Login
```
1. Open: http://localhost:5000/login.html
2. Enter test credentials (Admin account recommended)
3. Click "Login" button
4. Expected: Redirected to index.html with greeting
```

### Check Token Storage
```
Browser Console (F12):
> localStorage.getItem('token') 
// Should return JWT token starting with "eyJ..."
```

---

## 🍦 Ice Cream CRUD Testing

### Add Ice Cream
```
1. Go to: http://localhost:5000/ (index.html)
2. See: "Add Ice Cream" section at top
3. Enter: Name = "Vanilla Dream"
4. Click: "Add" button
5. Expected: 
   - Toast: "✓ Ice Cream added successfully!"
   - Table updates with new entry
   - Counter increments
```

**Advanced Test**: Try empty name
```
1. Leave name field empty
2. Click "Add" button
3. Expected: Toast warning "Please enter an ice cream name"
```

### Edit Ice Cream
```
1. In ice cream table, click "Edit" button on any row
2. Expected: Glassmorphic modal appears (blur background)
3. See: Form populated with current data
4. Edit: Change name to "Strawberry Swirl"
5. Check the "Milki" checkbox if not checked
6. Click: "Update" button
7. Expected:
   - Toast: "✓ Ice Cream updated successfully!"
   - Modal closes
   - Table reflects changes
```

**Animation Check**:
- Modal should slide up smoothly
- Background should have blur effect
- Buttons should have soft shadows

### Delete Ice Cream
```
1. In table, click "Delete" button
2. Expected: Browser confirmation dialog appears
3. Click "OK" on confirmation
4. Expected:
   - Toast: "✓ Operation successful!"
   - Table refreshes
   - Counter decrements
```

**Cancel Delete**:
```
1. Click "Delete" on any item
2. Click "Cancel" on confirmation dialog
3. Expected: Nothing happens, toast doesn't appear
```

---

## 👥 User Management CRUD Testing (Admin Only)

### Navigate to User Settings
```
1. Click "Users" link in top navigation
2. Expected: Redirected to http://localhost:5000/user.html
3. See: Two sections:
   - Admin: User Management table (top)
   - Profile: Edit current user (bottom)
```

### Add User
```
1. Scroll to "Add User" form
2. Enter:
   - Name: "John Doe"
   - Password: "password123"
   - Role: "User" (dropdown)
3. Click: "Add" button
4. Expected:
   - Toast: "✓ User added successfully!"
   - User appears in table below
   - Counter shows "2 users" (or updated count)
```

**Validation Test - Empty Name**:
```
1. Leave name empty
2. Click "Add"
3. Expected: Toast warning "Please enter a user name"
```

**Validation Test - Short Password**:
```
1. Enter Name: "Jane Doe"
2. Enter Password: "ab" (only 2 chars)
3. Click "Add"
4. Expected: Toast warning "Password must be at least 3 characters"
```

### Edit User
```
1. In user table, click "Edit" button
2. Expected: Glassmorphic modal appears
3. Fields populated:
   - Name: current name shown
   - Password: BLANK (for security)
   - Role: current role selected
4. Edit: Change name to "Jane Smith"
5. Password field: Leave blank (means don't change)
6. Click: "Update" button
7. Expected:
   - Toast: "✓ User updated successfully!"
   - Modal closes
   - Table shows updated name
```

**Change Password Test**:
```
1. Click "Edit" on a user
2. Name: leave as-is
3. Password: Enter "newpass123"
4. Click "Update"
5. Expected:
   - Toast success
   - You CAN log in as that user with new password
```

### Delete User
```
1. Click "Delete" button on a user row
2. Expected: Confirmation dialog
   "Are you sure you want to delete this user? This action cannot be undone."
3. Click "OK"
4. Expected:
   - Toast: "✓ User deleted successfully!"
   - User removed from table
   - Counter updates
```

---

## 👤 Profile Management Testing

### Update Your Profile
```
1. On user.html page, scroll to bottom
2. Click "Edit Profile" button
3. Expected: Glassmorphic form appears
4. Current values shown:
   - Name: Your current username
   - Password: BLANK
5. Edit: Change name
6. Optionally: Enter new password (if want to change)
7. Click: "Update" button
8. Expected:
   - Toast: "✓ Profile updated successfully!"
   - Form disappears if successful
   - Greeting in header updates immediately (if name changed)
```

---

## 🎨 UI/UX Testing

### Glassmorphism Verification
```
Check items to verify premium styling:

☐ Header: Frosted glass effect (white overlay, slight blur)
☐ Modals: Semi-transparent background with blur
☐ Tables: Soft shadows (not harsh borders)
☐ Buttons: Subtle shadows, smooth hover animation
☐ Cards: White background with professional spacing
☐ Toast Notifications: Smooth slide-in from right
```

### Color Palette Verification
```
☐ Primary Pink (#F4559E): Buttons, links, header border
☐ Secondary Orange (#F49F46): Secondary actions
☐ Tan Accent (#C4B88C): Tertiary elements
☐ Rose Background (#F2E8E6): Page background
☐ Deep Black (#190E0B): Text color
```

### Responsive Design Testing
```
Test at different screen sizes (F12 → Toggle Device):

Desktop (1920px):
☐ Full layout visible
☐ Tables properly spaced
☐ Header not wrapped

Tablet (768px):
☐ Header re-arranged
☐ Buttons stack if needed
☐ Forms responsive
☐ Tables scrollable

Mobile (375px):
☐ Single column layout
☐ Header hamburger-like
☐ Buttons full width
☐ Toast fits on screen
```

### Animation Testing
```
☐ Modal appears with smooth slide-up
☐ Buttons have hover lift effect
☐ Toasts slide in from right
☐ No janky or delayed animations
☐ Loading states smooth
```

---

## 🔔 Real-Time Updates Testing (SignalR)

### Connection Status
```
1. Open browser console (F12)
2. Look for messagected to activity hub"
3. In Network tab (F12):
   - See "activityHub" WebSocket connection
   - Status should be 101 (Switching Protocols)
```

### Activity Notifications
```
Multiiple Ways:
1. Open same app in TWO browser windows
2. In Window 1: Add/Edit/Delete an ice cream
3. In Window 2: Expected toast appears with activity
4. In Window 1: Table automatically refreshes
```

### Reconnection Testing
```
1. Open browser DevTools (F12)
2. Go to Network tab
3. Throttle connection: "Slow 3G"
4. Disconnect network cable or disable WiFi
5. Expected: Toast "Attempting to reconnect..."
6. Re-enable network
7. Expected: Toast "Reconnected to real-time updates"
8. Table refreshes automatically
```

---

## 🐛 Error Handling Testing

### Test 401 Unauthorized
```
1. Open DevTools Console
2. Run: localStorage.removeItem('token')
3. Try any CRUD operation (Add/Edit/Delete)
4. Expected: Redirected to login.html
```

### Test Network Error
```
1. Stop the .NET API server
2. Try any CRUD operation
3. Expected: Toast error message (not crash)
4. Start API server
5. Operations resume working
```

### Test Form Validation
```
✓ Empty name: "Please enter..." message
✓ Empty password: "Please enter password" message  
✓ Short password: "Password must be at least 3 characters"
✓ Invalid role: Form prevents invalid submissions
```

---

## 🔐 Security Verification

### Token Security
```
Console Check (F12):
> localStorage.getItem('token')
// Should be valid JWT (starts with eyJ)

> localStorage.getItem('token').split('.')[1]
// Decode payload to see claims
```

### Password Field Handling
```
1. Open DevTools (F12)
2. Click "Edit" on any ice cream
3. Close modal → Click "Edit" again
4. Expected: Form is fresh, no auto-filled previous values

5. Click "Edit" on a user
6. Expected: Password field is ALWAYS blank
7. Even if you edited that user before
```

### Authorization Test
```
If logged in as "User" role (non-admin):
1. Try to navigate to /user.html
2. Expected: Should see only your profile section
3. User table hidden or shows permission denied

If logged in as "Admin" role:
1. Navigate to /user.html
2. Expected: Full user management access
```

---

## 📊 Data Verification

### Check Server State
```
Via API directly (Postman or curl):

GET http://localhost:5000/api/IceCream
Headers: Authorization: Bearer {token}
Expected: Returns array of ice creams

GET http://localhost:5000/api/User  
Headers: Authorization: Bearer {token}
Expected: Returns array of users (admin only)
```

### Persistence Check
```
1. Add ice cream "Test1"
2. Refresh browser (F5)
3. Expected: "Test1" still in table (persisted to DB)
4. Close browser completely
5. Reopen and login
6. Expected: "Test1" still there
```

---

## 🎯 Test Scenarios

### Scenario 1: First-Time User
```
1. Clear cache (Ctrl+Shift+Delete)
2. Open http://localhost:5000/
3. Expected: Redirected to login.html
4. Login with valid credentials
5. Expected: Taken to index.html with greeting
6. Expected: Tables populated with data
7. Expected: SignalR connected toast appears
```

### Scenario 2: Complete CRUD Workflow
```
1. Login
2. Add ice cream "Chocolate"
3. Edit to "Dark Chocolate"
4. View in table - should show updated name
5. Delete "Dark Chocolate"
6. Expected: No longer in table
7. Logout
```

### Scenario 3: Administrative Tasks
```
1. Login as Admin
2. Go to Users page
3. Add test user "TestUser" with password
4. Edit to change role to "Admin"
5. Open new incognito window
6. Login as "TestUser" with new password
7. Expected: Can access system as Admin
8. Logout from both windows
```

---

## 📋 Checklist for Sign-Off

Before considering complete, verify:

- [ ] All 4 CRUD operations work (Add/Edit/Update/Delete)
- [ ] Glassmorphic styling visible (blur backgrounds, soft shadows)
- [ ] Forms validate input properly
- [ ] Toasts appear for success/error cases
- [ ] Table auto-refreshes after operations
- [ ] Pagination/scrolling works if tables are long
- [ ] Mobile responsive (test at 375px width)
- [ ] No JavaScript errors in console
- [ ] SignalR connection established
- [ ] Real-time updates work (test with 2 windows)
- [ ] Logout works and clears token
- [ ] Logging back in restores session
- [ ] Authentication redirects on 401
- [ ] Role-based access works (Admin vs User)
- [ ] Passwords handled securely (never shown in edit form by default)
- [ ] Page loads within reasonable time (<2 seconds)
- [ ] No visual glitches or layout shifts

---

## 🎓 Pro Tips

**Tip 1**: Use "Slow 3G" throttling (DevTools → Network) to test UI responsiveness

**Tip 2**: Test with browser zoom at 125% and 75% to verify responsive layout

**Tip 3**: Open DevTools in "Device Emulation" mode for mobile testing

**Tip 4**: Test with Screen Reader (Windows Narrator or NVDA) for accessibility

**Tip 5**: Use Lighthouse audit (DevTools → Lighthouse) for performance score

---

## ❓ FAQ

**Q: Why does the password field appear blank when editing?**
A: For security. We never pre-fill password fields. Leave blank to keep current password, or enter new one to change it.

**Q: Why does the table refresh automatically?**
A: Our improved SignalR integration automatically fetches latest data when notified of changes by other users.

**Q: Can I test without SignalR?**
A: Yes. App works fine without real-time updates. Tables will still refresh after you perform CRUD operations.

**Q: What if I get a 403 error?**
A: Your role doesn't have permission for that operation (e.g., non-admin trying to manage users).

**Q: Why is the CSS not loading?**
A: Hard refresh (Ctrl+F5) to clear browser cache. If persistent, check Network tab for 404 on site.css.

---

**Happy Testing! 🎉**
**Version**: Professional v1.0
**Status**: Ready for Production
