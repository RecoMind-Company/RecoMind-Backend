# CRUD Operations - Complete Implementation Guide

## Overview

This guide covers the complete implementation of Create, Read, Update, and Delete (CRUD) operations for the SignalR Comment Hub with WhatsApp-style timestamps and 5-minute edit timeout.

---

## 1. CREATE - Send Comment

### Frontend (JavaScript)

```javascript
async function sendComment() {
    // Check connection status
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        alert('Not connected');
        return;
    }

    // Get comment text
    const comment = document.getElementById('userComment').value.trim();
    
    // Validate not empty
    if (!comment) {
        alert('Please enter a comment');
        return;
    }

    try {
        // Send to server via SignalR
        await connection.invoke('CreateComment', { 
            userComment: comment 
        });

        // Clear input
        document.getElementById('userComment').value = '';
        document.getElementById('charCount').textContent = '0/500';
        document.getElementById('validationErrors').style.display = 'none';
    } catch (error) {
        log(`? Error: ${error.message}`);
    }
}
```

### Backend (.NET 8)

```csharp
// In CommentHub
public async Task CreateComment(AddCommentDto addCommentDto)
{
    var userId = Context.User?.Claims.FirstOrDefault(
        c => c.Type == ClaimTypes.NameIdentifier)?.Value;
    addCommentDto.UserId = userId;
    addCommentDto.PlanId = Context.Items["planId"] as string;
    
    // Validate and save
    var result = await commentService.AddCommentAsync(addCommentDto);
    
    if (result.IsSuccess)
    {
        // Broadcast to all users in plan group
        await Clients.Group(addCommentDto.PlanId!)
            .SendAsync("ReceiveComment", result.Value);
    }
}
```

### Data Model

```csharp
public class Comment
{
    public string Id { get; set; }
    public string UserComment { get; set; }
    public string UserId { get; set; }
    public string PlanId { get; set; }
    
    // ? Automatically set on creation
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Updated only on edit
    public DateTime? UpdatedAt { get; set; }
}
```

### Response DTO

```csharp
public class CommentDto
{
    public string Id { get; set; }
    public string UserComment { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // ? Computed property: true if UpdatedAt is not null
    public bool IsUpdated => UpdatedAt != null;
}
```

### Flow Diagram

```
User Types Comment
    ?
Click "Send Comment"
    ?
Validation (not empty, ? 500 chars)
    ?
SignalR invoke CreateComment()
    ?
Backend: AddCommentAsync()
    ?
Set CreatedAt = DateTime.UtcNow
    ?
Save to Database
    ?
Broadcast ReceiveComment event
    ?
All clients receive updated comment
    ?
Add to UI with date separator
    ?
Start edit timeout monitor (5 min)
```

---

## 2. READ - Display Comments

### Frontend (JavaScript)

```javascript
function addCommentToList(comment) {
    const list = document.getElementById('commentsList');

    // Clear "no comments" message
    if (list.children.length === 1 && list.children[0].textContent.includes('No comments')) {
        list.innerHTML = '';
        lastDateSeparator = null;
    }

    // Update counter
    messageCount++;
    document.getElementById('messageCount').textContent = 
        `${messageCount} message${messageCount !== 1 ? 's' : ''}`;

    // Store in map
    commentsMap.set(comment.id, comment);

    // Format display data
    const userIdShort = comment.userId.substring(0, 3).toUpperCase();
    const isOwnComment = comment.userId === currentUserId;
    const createdTime = formatTime(comment.createdAt);
    const dateKey = getDateKey(comment.createdAt);
    const dateDisplay = formatDate(comment.createdAt);
    
    // ? Check if editable (within 5 minutes)
    const canEdit = isOwnComment && isEditableWithinTimeout(comment.createdAt);

    // Add date separator if date changed
    let dateSeparatorHTML = '';
    if (dateKey && dateKey !== lastDateSeparator) {
        dateSeparatorHTML = `
            <div class="date-separator">
                <span class="date-separator-text">${dateDisplay}</span>
            </div>
        `;
        lastDateSeparator = dateKey;
    }

    // Build action buttons (only for own comments)
    const actionButtonsHTML = isOwnComment ? `
        ${canEdit ? `<button class="btn-edit btn-small" onclick="startEditComment('${comment.id}')">?? Edit</button>` : ''}
        <button class="btn-delete btn-small" onclick="deleteComment('${comment.id}')">??? Delete</button>
    ` : '';

    // ? Show warning if edit timeout expired
    const editTimeoutWarning = isOwnComment && !canEdit ? 
        `<div class="edit-timeout-warning">?? Edit time expired (5 min limit)</div>` : '';

    // Build HTML
    const itemHTML = `
        ${dateSeparatorHTML}
        <div class="message-item" id="comment-${comment.id}">
            <div class="message-meta-small">
                <div class="user-badge">${userIdShort}</div>
                <div class="time-badge" id="time-${comment.id}">${createdTime}</div>
            </div>
            <div class="message-body">
                <div class="message-header">
                    <span class="username-badge">${comment.username || 'Anonymous'}</span>
                    ${comment.isUpdated ? '<span class="edited-badge">[EDITED]</span>' : ''}
                </div>
                <div class="message-content" id="content-${comment.id}">
                    ${escapeHtml(comment.userComment)}
                </div>
                <div class="edit-form" id="edit-${comment.id}">
                    <textarea id="edit-text-${comment.id}" placeholder="Edit comment...">
                        ${escapeHtml(comment.userComment)}
                    </textarea>
                    <div class="edit-buttons">
                        <button class="btn-save btn-small" onclick="saveEditComment('${comment.id}')">? Save</button>
                        <button class="btn-cancel btn-small" onclick="cancelEditComment('${comment.id}')">? Cancel</button>
                    </div>
                </div>
                <div class="message-actions">
                    ${actionButtonsHTML}
                </div>
                ${editTimeoutWarning}
            </div>
        </div>
    `;

    // Insert into DOM
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = itemHTML;
    while (tempDiv.firstChild) {
        list.insertBefore(tempDiv.firstChild, list.firstChild);
    }

    // ? Start edit timeout monitor
    if (isOwnComment) {
        setupEditTimeoutMonitor(comment.id, comment.createdAt);
    }
}
```

### Timeline Display

```
CreatedAt: 2025-03-26 14:30:00
UpdatedAt: null
isUpdated: false

Display:
????????????????????
    Today
????????????????????
[ABC] 14:30  Hello world
[?? Edit] [??? Delete]
```

---

## 3. UPDATE - Edit Comment (with 5-Minute Timeout)

### Key Feature: 5-Minute Edit Window

**Backend Validation:**
```csharp
public async Task<Result<CommentDto>> UpdateCommentAsync(UpdateCommentDto updateCommentDto)
{
    var comment = await _commentRepository.Find(c => c.Id == updateCommentDto.CommentId);

    if (comment == null)
        return Result<CommentDto>.Failure(CommentErrors.NotFound);
    
    if (comment.UserId != updateCommentDto.UserId)
        return Result<CommentDto>.Failure(CommentErrors.AccessDenied);
    
    // ? CRITICAL: Validate 5-minute edit window
    if (DateTime.UtcNow > comment.CreatedAt.AddMinutes(5))
        return Result<CommentDto>.Failure(CommentErrors.EditTimeout);
    
    // Update comment text
    mapper.Map(updateCommentDto, comment);
    
    // ? Set UpdatedAt to now
    comment.UpdatedAt = DateTime.UtcNow;
    
    _commentRepository.Update(comment);
    await unitOfWork.SaveChangesAsync();

    var commentDto = mapper.Map<CommentDto>(comment);
    return Result<CommentDto>.Success(commentDto);
}
```

### Frontend - Edit Process

```javascript
// Step 1: User clicks Edit button
function startEditComment(commentId) {
    const contentEl = document.getElementById(`content-${commentId}`);
    const editForm = document.getElementById(`edit-${commentId}`);
    const textArea = document.getElementById(`edit-text-${commentId}`);

    // Hide message, show edit form
    if (contentEl) contentEl.style.display = 'none';
    if (editForm) editForm.classList.add('active');
    if (textArea) textArea.focus();
}

// Step 2: User clicks Save button
async function saveEditComment(commentId) {
    // Validate connection
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        alert('Not connected');
        return;
    }

    // Get new text
    const textArea = document.getElementById(`edit-text-${commentId}`);
    const newComment = textArea.value.trim();

    // ? Validation 1: Not empty
    if (!newComment) {
        alert('Comment cannot be empty');
        return;
    }

    // ? Validation 2: Max 500 characters
    if (newComment.length > 500) {
        alert('Comment must be 500 characters or less');
        return;
    }

    try {
        // Show loading state
        const saveButton = event.target;
        const originalText = saveButton.textContent;
        saveButton.textContent = '? Saving...';
        saveButton.disabled = true;

        // ? Send to server
        await connection.invoke('EditComment', {
            commentId: commentId,
            userComment: newComment
        });

        log(`? Edit sent for comment: ${commentId}`);
        
        // Reset button
        saveButton.textContent = originalText;
        saveButton.disabled = false;

        // ? Immediately hide edit form (will be updated via event)
        const editForm = document.getElementById(`edit-${commentId}`);
        const contentEl = document.getElementById(`content-${commentId}`);
        if (editForm) editForm.classList.remove('active');
        if (contentEl) contentEl.style.display = 'block';

    } catch (error) {
        log(`? Error saving edit: ${error.message}`);
        
        // Reset button
        const saveButton = event.target;
        saveButton.textContent = '?? Save';
        saveButton.disabled = false;

        // Show error
        alert(`Error: ${error.message}`);
    }
}
```

### Frontend - Real-Time Edit Timeout Monitor

```javascript
// ? NEW: Monitor edit timeout without page refresh
const editTimeoutIntervals = new Map(); // Track intervals per comment

function setupEditTimeoutMonitor(commentId, createdAtIso) {
    // Clear existing interval
    if (editTimeoutIntervals.has(commentId)) {
        clearInterval(editTimeoutIntervals.get(commentId));
    }

    // Check every 1 second
    const intervalId = setInterval(() => {
        const canEdit = isEditableWithinTimeout(createdAtIso);
        updateEditButtonState(commentId, canEdit);

        // Stop after timeout expires
        if (!canEdit) {
            clearInterval(intervalId);
            editTimeoutIntervals.delete(commentId);
            log(`? Edit timeout expired for comment: ${commentId}`);
        }
    }, 1000); // Check every second

    editTimeoutIntervals.set(commentId, intervalId);
}

function updateEditButtonState(commentId, canEdit) {
    const item = document.getElementById(`comment-${commentId}`);
    if (!item) return;

    const actionContainer = item.querySelector('.message-actions');
    
    if (canEdit) {
        // ? Show edit button if not present
        if (!item.querySelector('.btn-edit')) {
            const editButton = document.createElement('button');
            editButton.className = 'btn-edit btn-small';
            editButton.onclick = () => startEditComment(commentId);
            editButton.innerHTML = '?? Edit';
            
            // Insert before delete button
            const deleteButton = actionContainer.querySelector('.btn-delete');
            if (deleteButton) {
                deleteButton.parentNode.insertBefore(editButton, deleteButton);
            } else {
                actionContainer.appendChild(editButton);
            }

            log(`? Edit button shown for comment: ${commentId}`);
        }

        // Remove warning if present
        const existingWarning = item.querySelector('.edit-timeout-warning');
        if (existingWarning) {
            existingWarning.remove();
        }
    } else {
        // ? Hide edit button
        const editButton = item.querySelector('.btn-edit');
        if (editButton) {
            editButton.remove();
            log(`? Edit button removed for comment: ${commentId}`);
        }

        // ? Show warning
        if (!item.querySelector('.edit-timeout-warning')) {
            const messageBody = item.querySelector('.message-body');
            const warningDiv = document.createElement('div');
            warningDiv.className = 'edit-timeout-warning';
            warningDiv.textContent = '?? Edit time expired (5 min limit)';
            messageBody.appendChild(warningDiv);

            log(`?? Warning shown for comment: ${commentId}`);
        }
    }
}

function isEditableWithinTimeout(createdAtIso) {
    try {
        const createdAt = new Date(createdAtIso);
        const now = new Date();
        const diffMinutes = (now - createdAt) / (1000 * 60);
        return diffMinutes < 5;
    } catch (e) {
        return false;
    }
}
```

### SignalR Event - Update Broadcast

```javascript
connection.on('ReceiveEditedComment', (comment) => {
    log(`?? Comment edited by ${comment.username} at ${formatTime(comment.updatedAt)}`);
    updateCommentInList(comment);
});
```

### Frontend - Update UI

```javascript
function updateCommentInList(comment) {
    const item = document.getElementById(`comment-${comment.id}`);
    if (!item) return;

    // ? Update message text
    const contentEl = document.getElementById(`content-${comment.id}`);
    if (contentEl) {
        contentEl.textContent = comment.userComment;
        contentEl.style.display = 'block';
    }

    // ? Update time to new edit time
    const timeEl = document.getElementById(`time-${comment.id}`);
    if (timeEl) {
        timeEl.textContent = formatTime(comment.updatedAt);
    }

    // Close edit form
    const editForm = document.getElementById(`edit-${comment.id}`);
    if (editForm && editForm.classList.contains('active')) {
        editForm.classList.remove('active');
    }

    // ? Add edited class
    item.classList.add('edited');

    // ? Add [EDITED] badge if not present
    const header = item.querySelector('.message-header');
    if (!header.querySelector('.edited-badge')) {
        const badge = document.createElement('span');
        badge.className = 'edited-badge';
        badge.textContent = '[EDITED]';
        header.appendChild(badge);
    }

    log(`? Comment updated: ${comment.id} - Time: ${formatTime(comment.updatedAt)}`);
}
```

### Update Timeline

```
BEFORE:
CreatedAt: 14:30:00
UpdatedAt: null
isUpdated: false
Display: [ABC] 14:30  Hello

User clicks Edit at 14:32
Types "Hi there" and clicks Save

AFTER:
CreatedAt: 14:30:00 (unchanged)
UpdatedAt: 14:32:45 (set on edit)
isUpdated: true
Display: [ABC] 14:32  Hi there [EDITED]
         ?? Edit time expired (after 5 min)
```

---

## 4. DELETE - Remove Comment

### Frontend (JavaScript)

```javascript
async function deleteComment(commentId) {
    // Validate connection
    if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
        alert('Not connected');
        return;
    }

    // Confirm deletion
    if (!confirm('Are you sure you want to delete this comment?')) {
        return;
    }

    try {
        log(`??? Deleting comment: ${commentId}`);
        
        // Send delete request
        await connection.invoke('DeleteComment', commentId);
        
        log(`? Delete request sent for comment: ${commentId}`);
    } catch (error) {
        log(`? Error deleting comment: ${error.message}`);
        alert(`Error: ${error.message}`);
    }
}
```

### Backend (.NET 8)

```csharp
// In CommentHub
public async Task DeleteComment(string commentId)
{
    var userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);
    var planId = Context.Items["planId"] as string;
    
    var result = await commentService.DeleteCommentAsync(commentId, userId!);
    
    await result.MapAsync(
        // Success: Broadcast deletion to group
        onSuccess: _ => Clients.Group(planId!).SendAsync(
            "ReceiveDeletedComment", 
            commentId, 
            "This comment has been deleted."
        ),
        // Failure: Send error to caller
        onFailure: errors => Clients.Caller.SendAsync("ReceiveErrors", errors)
    );
}
```

### Service Layer

```csharp
public async Task<Result<bool>> DeleteCommentAsync(string commentId, string userId)
{
    var comment = await _commentRepository.Find(c => c.Id == commentId);

    if (comment == null)
        return Result<bool>.Failure(CommentErrors.NotFound);
    
    if (comment.UserId != userId)
        return Result<bool>.Failure(CommentErrors.AccessDenied);

    _commentRepository.Delete(comment);
    await unitOfWork.SaveChangesAsync();

    return Result<bool>.Success(true);
}
```

### Frontend - Remove from UI

```javascript
function deleteCommentFromList(commentId) {
    // ? Clear timeout monitor interval
    if (editTimeoutIntervals.has(commentId)) {
        clearInterval(editTimeoutIntervals.get(commentId));
        editTimeoutIntervals.delete(commentId);
    }

    const item = document.getElementById(`comment-${commentId}`);
    if (!item) return;

    // Remove from map
    commentsMap.delete(commentId);
    
    // Start fade-out animation
    item.classList.add('deleting');

    // Remove after animation
    setTimeout(() => {
        item.remove();
        messageCount--;
        document.getElementById('messageCount').textContent = 
            `${messageCount} message${messageCount !== 1 ? 's' : ''}`;

        // Show empty state if no more comments
        const list = document.getElementById('commentsList');
        if (list.children.length === 0) {
            list.innerHTML = '<div style="color: #999; text-align: center; padding: 20px;">No comments yet...</div>';
            lastDateSeparator = null;
        }
    }, 300);

    log(`??? Comment deleted: ${commentId}`);
}
```

---

## 5. Error Handling

### Error Types

```javascript
connection.on('ReceiveValidationErrors', (errors) => {
    // Validation errors (empty comment, etc.)
    displayValidationErrors(errors);
});

connection.on('ReceiveErrors', (errors) => {
    // Server errors (access denied, edit timeout, etc.)
    log(`? Server error: ${JSON.stringify(errors)}`);
    alert(`Error: ${errors[0]?.message || 'Unknown error'}`);
});
```

### Error Classes

```csharp
internal static class CommentErrors
{
    internal static Error NotFound => 
        new("Comment.NotFound", "Comment isn't found");
    
    internal static Error AccessDenied => 
        new("Comment.AccessDenied", "You aren't allowed to perform this action on the comment");
    
    internal static Error EditTimeout => 
        new("Comment.EditTimeout", "you can't edit this comment now");
}
```

---

## 6. Complete Flow Diagram

```
???????????????????????????????????????????????????????????????????
?                         USER ACTIONS                            ?
???????????????????????????????????????????????????????????????????
                    ?
        ????????????????????????????????????????
        ?           ?           ?              ?
        ?           ?           ?              ?
     CREATE      READ       UPDATE           DELETE
        ?           ?           ?              ?
    Send Msg   Display     Click Edit     Click Delete
        ?       with Date    Edit Text       Confirm
        ?       Separator    Save Changes      ?
        ?           ?           ?              ?
        ????????????????????????????????????????
                    ?
        ????????????????????????????????????
        ?   SignalR Hub (CommentHub)       ?
        ?   - Validate user                ?
        ?   - Check permissions            ?
        ?   - Validate 5-min timeout       ?
        ?   - Save to database             ?
        ????????????????????????????????????
                    ?
        ????????????????????????????????????
        ?        Database                  ?
        ?   - CreatedAt (immutable)        ?
        ?   - UpdatedAt (set on edit)      ?
        ?   - IsUpdated (computed)         ?
        ????????????????????????????????????
                    ?
        ????????????????????????????????????
        ?  Broadcast to Group (PlanId)    ?
        ?  - ReceiveComment               ?
        ?  - ReceiveEditedComment         ?
        ?  - ReceiveDeletedComment        ?
        ????????????????????????????????????
                    ?
        ????????????????????????????????????
        ?    All Connected Clients        ?
        ?  - Update UI                    ?
        ?  - Show date separators         ?
        ?  - Monitor edit timeout         ?
        ?  - Remove deleted messages      ?
        ?????????????????????????????????????
```

---

## 7. Testing Scenarios

### Test 1: Create Comment
? Send new comment
? Verify CreatedAt is set
? Verify message appears immediately
? Verify date separator shows

### Test 2: Read/Display
? Multiple comments display correctly
? Date separators show on date change
? Time-only format (HH:MM)
? Own comments show Edit/Delete buttons
? Others' comments show neither

### Test 3: Edit Within 5 Minutes
? Edit button visible
? Click Edit to open form
? Click Save to send edit
? Message text updates
? Time updates to edit time
? [EDITED] badge appears
? Background changes to blue

### Test 4: Edit After 5 Minutes
? Edit button disappears
? Warning message shows
? Delete button still available
? No visual changes after timeout

### Test 5: Delete Comment
? Click Delete
? Confirm dialog appears
? Message fades out
? Interval cleared (no memory leak)
? Message count updates

---

## 8. Key Improvements (v2)

### Fixed Issues
- ? **Save button now works**: Shows loading state, closes form immediately
- ? **Real-time timeout**: Edit button disappears after 5 minutes without refresh
- ? **Better feedback**: Visual confirmation when saving
- ? **Error handling**: Proper error messages on failure
- ? **Memory management**: Intervals cleared when comments deleted

### New Features
- ? **Edit timeout monitor**: Real-time update without page refresh
- ? **Loading state**: "Saving..." indicator on save button
- ? **Immediate feedback**: Edit form closes right away
- ? **Better UX**: Warning message when timeout expires

---

## Database Schema

```sql
CREATE TABLE Comments (
    Id NVARCHAR(36) PRIMARY KEY,
    UserComment NVARCHAR(500) NOT NULL,
    UserId NVARCHAR(MAX) NOT NULL,
    PlanId NVARCHAR(MAX) NOT NULL,
    
    -- ? Timestamps
    CreatedAt DATETIME2 NOT NULL,      -- Set once on creation
    UpdatedAt DATETIME2 NULL,          -- Set on edit only
    
    FOREIGN KEY (PlanId) REFERENCES Plans(Id)
);
```

---

**Version:** 2.0  
**Status:** ? Production Ready  
**Last Updated:** 2025-03-26  
**Framework:** .NET 8 + SignalR + JavaScript
