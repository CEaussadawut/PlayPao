function ok() {
    console.log("OK");
}


function toggleNotificationBox() {
    const box = document.getElementById("notification-box");
    box.classList.toggle("hidden");
    if (!box.classList.contains("hidden")) {
        loadNotifications();
        resetNotificationCount();
    }
}

function loadNotifications() {
    var xhrTotal = new XMLHttpRequest();
    xhrTotal.open('GET', '/Notification/GetTotalCount', true);
    xhrTotal.onreadystatechange = function () {
        if (xhrTotal.readyState === 4 && xhrTotal.status === 200) {
            var totalCount = JSON.parse(xhrTotal.responseText);
            if (previousTotalCount > 0 && totalCount > previousTotalCount && !hasReloadedForNewNotifications) {
                hasReloadedForNewNotifications = true;
                location.reload();
                return;
            }
            previousTotalCount = totalCount;
        }
    };
    xhrTotal.send();

    var xhr = new XMLHttpRequest();
    xhr.open('GET', '/Notification/GetUnreadCount', true);
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4 && xhr.status === 200) {
            var count = JSON.parse(xhr.responseText);
            const badge = document.getElementById("notification-badge");
            if (count > 0) {
                badge.textContent = count;
                badge.style.display = "inline";
            } else {
                badge.style.display = "none";
            }

            previousUnreadCount = count;
        }
    };
    xhr.send();

    var xhr2 = new XMLHttpRequest();
    xhr2.open('GET', '/Notification/Index', true);
    xhr2.onreadystatechange = function () {
        if (xhr2.readyState === 4 && xhr2.status === 200) {
            var html = xhr2.responseText;
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

                        if (notificationText.includes('has been approved')) {
                            clonedItem.style.cursor = 'pointer';
                            clonedItem.addEventListener('click', function () {
                                window.location.href = '/Event/Detail/' + this.dataset.eventId;

                                const notificationId = this.dataset.id;
                                if (notificationId) {
                                    var xhrInner = new XMLHttpRequest();
                                    xhrInner.open('POST', '/Notification/MarkAsRead', true);
                                    xhrInner.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                                    xhrInner.send('id=' + notificationId);
                                }
                            });
                        } else if (notificationText.includes('has been rejected')) {
                            clonedItem.style.cursor = 'default';
                            clonedItem.style.opacity = '0.7';
                        } else if (notificationText.includes('requested to join')) {
                            clonedItem.style.cursor = 'pointer';
                            clonedItem.addEventListener('click', function () {
                                window.location.href = '/Event/Pending';

                                const notificationId = this.dataset.id;
                                if (notificationId) {
                                    var xhrInner = new XMLHttpRequest();
                                    xhrInner.open('POST', '/Notification/MarkAsRead', true);
                                    xhrInner.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                                    xhrInner.send('id=' + notificationId);
                                }
                            });
                        } else {
                            clonedItem.style.cursor = 'pointer';
                            clonedItem.addEventListener('click', function () {
                                window.location.href = '/Notification';

                                const notificationId = this.dataset.id;
                                if (notificationId) {
                                    var xhrInner = new XMLHttpRequest();
                                    xhrInner.open('POST', '/Notification/MarkAsRead', true);
                                    xhrInner.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                                    xhrInner.send('id=' + notificationId);
                                }
                            });
                        }

                        list.appendChild(clonedItem);
                    }
                });
            } else {
                list.innerHTML = '<div class="notification-item"><p>No new notifications</p></div>';
            }
        }
    };
    xhr2.send();
}

function resetNotificationCount() {
    var xhr3 = new XMLHttpRequest();
    xhr3.open('GET', '/Notification/Index', true);
    xhr3.onreadystatechange = function () {
        if (xhr3.readyState === 4 && xhr3.status === 200) {
            var html = xhr3.responseText;
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, 'text/html');
            const unreadItems = doc.querySelectorAll('.notification-item.unread');

            unreadItems.forEach(item => {
                const notificationId = item.dataset.id;
                if (notificationId) {
                    var xhrInner2 = new XMLHttpRequest();
                    xhrInner2.open('POST', '/Notification/MarkAsRead', true);
                    xhrInner2.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');
                    xhrInner2.send('id=' + notificationId);
                }
            });

            setTimeout(() => {
                var xhr4 = new XMLHttpRequest();
                xhr4.open('GET', '/Notification/GetUnreadCount', true);
                xhr4.onreadystatechange = function () {
                    if (xhr4.readyState === 4 && xhr4.status === 200) {
                        var count = JSON.parse(xhr4.responseText);
                        const badge = document.getElementById("notification-badge");
                        if (count > 0) {
                            badge.textContent = count;
                            badge.style.display = "inline";
                        } else {
                            badge.style.display = "none";
                        }
                    }
                };
                xhr4.send();
            }, 100);
        }
    };
    xhr3.send();
}

let previousUnreadCount = 0;
let previousTotalCount = 0;
let hasReloadedForNewNotifications = false;

function startNotificationPolling() {
    if (document.getElementById("notification-badge")) {
        loadNotifications();
        setInterval(function () {
            loadNotifications();
        }, 1000);
    }
}

document.addEventListener("DOMContentLoaded", function () {
    startNotificationPolling();

    document.addEventListener("click", function (e) {
        const box = document.getElementById("notification-box");
        const icon = document.querySelector(".notification-icon");

        if (box && icon && !box.contains(e.target) && !icon.contains(e.target)) {
            box.classList.add("hidden");
        }
    });

    if (window.lucide) lucide.createIcons();
});