document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.getElementById('toggleSidebar');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('main-content');
    const menuIcon = document.getElementById('menuIcon');

    toggleBtn.addEventListener('click', () => {
        sidebar.classList.toggle('active');
        mainContent.classList.toggle('shifted');

        if (sidebar.classList.contains('active')) {
            menuIcon.textContent = 'menu_open';
        } else {
            menuIcon.textContent = 'menu';
        }
    });
});