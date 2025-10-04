// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

function ok() {
    console.log("OK");
}


function toggleNotificationBox() {
    const box = document.getElementById("notification-box");
    box.classList.toggle("hidden");
    if (!box.classList.contains("hidden")) {
        loadNotifications();
        // Reset notification count when opening
        resetNotificationCount();
    }
}

function loadNotifications() {
    fetch('/Notification/GetUnreadCount')
        .then(response => response.json())
        .then(count => {
            const badge = document.getElementById("notification-badge");
            if (count > 0) {
                badge.textContent = count;
                badge.style.display = "inline";
            } else {
                badge.style.display = "none";
            }
        });

    // Load recent notifications for dropdown
    fetch('/Notification/Index')
        .then(response => response.text())
        .then(html => {
            // Extract notification items from the full page
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const notifications = doc.querySelectorAll('.notification-item');
            const list = document.getElementById("notification-list");

            list.innerHTML = '';
            if (notifications.length > 0) {
                notifications.forEach((item, index) => {
                    if (index < 5) { // Show only first 5 in dropdown
                        const clonedItem = item.cloneNode(true);
                        const notificationText = clonedItem.querySelector('p').textContent;

                        // Only make clickable if it's an approval notification
                        if (notificationText.includes('has been approved')) {
                            clonedItem.style.cursor = 'pointer';
                            clonedItem.addEventListener('click', function () {
                                // Go to event detail page for approved requests
                                window.location.href = '/Event/Detail/' + this.dataset.eventId;

                                // Mark as read when clicked
                                const notificationId = this.dataset.id;
                                if (notificationId) {
                                    fetch('/Notification/MarkAsRead', {
                                        method: 'POST',
                                        headers: {
                                            'Content-Type': 'application/x-www-form-urlencoded',
                                        },
                                        body: 'id=' + notificationId
                                    });
                                }
                            });
                        } else if (notificationText.includes('has been rejected')) {
                            // Rejection notifications are not clickable
                            clonedItem.style.cursor = 'default';
                            clonedItem.style.opacity = '0.7';
                        } else if (notificationText.includes('requested to join')) {
                            // Join request notifications go to pending page
                            clonedItem.style.cursor = 'pointer';
                            clonedItem.addEventListener('click', function () {
                                window.location.href = '/Event/Pending';

                                // Mark as read when clicked
                                const notificationId = this.dataset.id;
                                if (notificationId) {
                                    fetch('/Notification/MarkAsRead', {
                                        method: 'POST',
                                        headers: {
                                            'Content-Type': 'application/x-www-form-urlencoded',
                                        },
                                        body: 'id=' + notificationId
                                    });
                                }
                            });
                        } else {
                            // Other notifications go to notification page
                            clonedItem.style.cursor = 'pointer';
                            clonedItem.addEventListener('click', function () {
                                window.location.href = '/Notification';

                                // Mark as read when clicked
                                const notificationId = this.dataset.id;
                                if (notificationId) {
                                    fetch('/Notification/MarkAsRead', {
                                        method: 'POST',
                                        headers: {
                                            'Content-Type': 'application/x-www-form-urlencoded',
                                        },
                                        body: 'id=' + notificationId
                                    });
                                }
                            });
                        }

                        list.appendChild(clonedItem);
                    }
                });
            } else {
                list.innerHTML = '<div class="notification-item"><p>No new notifications</p></div>';
            }
        });
}

function resetNotificationCount() {
    // Mark all unread notifications as read
    fetch('/Notification/Index')
        .then(response => response.text())
        .then(html => {
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const unreadItems = doc.querySelectorAll('.notification-item.unread');

            // Mark all unread notifications as read
            unreadItems.forEach(item => {
                const notificationId = item.dataset.id;
                if (notificationId) {
                    fetch('/Notification/MarkAsRead', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/x-www-form-urlencoded',
                        },
                        body: 'id=' + notificationId
                    });
                }
            });

            // Update badge count after a short delay
            setTimeout(() => {
                fetch('/Notification/GetUnreadCount')
                    .then(response => response.json())
                    .then(count => {
                        const badge = document.getElementById("notification-badge");
                        if (count > 0) {
                            badge.textContent = count;
                            badge.style.display = "inline";
                        } else {
                            badge.style.display = "none";
                        }
                    });
            }, 100);
        });
}

document.addEventListener("DOMContentLoaded", function () {
    // Load notification count on page load
    if (document.getElementById("notification-badge")) {
        loadNotifications();
    }

    // Close notification box when clicking outside
    document.addEventListener("click", function (e) {
        const box = document.getElementById("notification-box");
        const icon = document.querySelector(".notification-icon");

        if (box && icon && !box.contains(e.target) && !icon.contains(e.target)) {
            box.classList.add("hidden");
        }
    });

    if (window.lucide) lucide.createIcons();
});