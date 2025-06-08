document.addEventListener("DOMContentLoaded", function () {
    const toggleBtn = document.getElementById('toggleSidebar');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('main-content');
    const menuIcon = document.getElementById('menuIcon');

    const sidebarState = localStorage.getItem('sidebarState');
    if (sidebarState === 'open') {
        sidebar.classList.add('active');
        mainContent.classList.add('shifted');
        menuIcon.textContent = 'menu_open';
    }

    toggleBtn.addEventListener('click', () => {
        const isActive = sidebar.classList.toggle('active');
        mainContent.classList.toggle('shifted');

        localStorage.setItem('sidebarState', isActive ? 'open' : 'closed');

        menuIcon.textContent = isActive ? 'menu_open' : 'menu';
    });
});
