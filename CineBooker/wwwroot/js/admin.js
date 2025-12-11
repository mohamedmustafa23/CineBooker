document.addEventListener('DOMContentLoaded', function() {
    initSidebar();
    initDarkMode();
    initDataTables();
    initDeleteConfirmation();
    initImagePreview();
    initFormValidation();
    initTooltips();
});

function initSidebar() {
    const sidebar = document.getElementById('sidebar');
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebarClose = document.getElementById('sidebarClose');
    
    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener('click', function() {
            sidebar.classList.toggle('show');
            toggleOverlay(true);
        });
    }
    
    if (sidebarClose && sidebar) {
        sidebarClose.addEventListener('click', function() {
            sidebar.classList.remove('show');
            toggleOverlay(false);
        });
    }
    
    document.addEventListener('click', function(e) {
        if (window.innerWidth < 992) {
            if (!sidebar.contains(e.target) && !sidebarToggle.contains(e.target)) {
                sidebar.classList.remove('show');
                toggleOverlay(false);
            }
        }
    });
}

function toggleOverlay(show) {
    let overlay = document.querySelector('.sidebar-overlay');
    if (!overlay && show) {
        overlay = document.createElement('div');
        overlay.className = 'sidebar-overlay';
        document.body.appendChild(overlay);
        overlay.addEventListener('click', function() {
            document.getElementById('sidebar').classList.remove('show');
            toggleOverlay(false);
        });
    }
    if (overlay) {
        overlay.classList.toggle('show', show);
    }
}

function initDarkMode() {
    const darkModeToggle = document.getElementById('darkModeToggle');
    const themeIcon = document.getElementById('themeIcon');
    const html = document.documentElement;
    
    const savedTheme = localStorage.getItem('theme') || 'light';
    html.setAttribute('data-bs-theme', savedTheme);
    if (darkModeToggle) {
        darkModeToggle.checked = savedTheme === 'dark';
    }
    updateThemeIcon(savedTheme);
    
    if (darkModeToggle) {
        darkModeToggle.addEventListener('change', function() {
            const theme = this.checked ? 'dark' : 'light';
            html.setAttribute('data-bs-theme', theme);
            localStorage.setItem('theme', theme);
            updateThemeIcon(theme);
        });
    }
}

function updateThemeIcon(theme) {
    const themeIcon = document.getElementById('themeIcon');
    if (themeIcon) {
        themeIcon.className = theme === 'dark' ? 'bi bi-sun' : 'bi bi-moon-stars';
    }
}

function initDataTables() {
    const tables = document.querySelectorAll('.data-table');
    tables.forEach(function(table) {
        if ($.fn.DataTable.isDataTable(table)) {
            return;
        }
        $(table).DataTable({
            responsive: true,
            pageLength: 10,
            lengthMenu: [[10, 25, 50, 100, -1], [10, 25, 50, 100, "All"]],
            language: {
                search: "_INPUT_",
                searchPlaceholder: "Search...",
                lengthMenu: "Show _MENU_ entries",
                info: "Showing _START_ to _END_ of _TOTAL_ entries",
                paginate: {
                    first: '<i class="bi bi-chevron-double-left"></i>',
                    last: '<i class="bi bi-chevron-double-right"></i>',
                    next: '<i class="bi bi-chevron-right"></i>',
                    previous: '<i class="bi bi-chevron-left"></i>'
                }
            },
            dom: '<"row"<"col-sm-12 col-md-6"l><"col-sm-12 col-md-6"f>>rtip'
        });
    });
}

function initDeleteConfirmation() {
    const deleteModal = document.getElementById('confirmDeleteModal');
    if (!deleteModal) return;
    
    document.querySelectorAll('[data-delete-url]').forEach(function(btn) {
        btn.addEventListener('click', function(e) {
            e.preventDefault();
            const url = this.getAttribute('data-delete-url');
            const name = this.getAttribute('data-delete-name') || 'this item';
            
            document.getElementById('deleteConfirmMessage').textContent = 
                `Are you sure you want to delete "${name}"? This action cannot be undone.`;
            document.getElementById('deleteForm').setAttribute('action', url);
            
            const modal = new bootstrap.Modal(deleteModal);
            modal.show();
        });
    });
}

function initImagePreview() {
    document.querySelectorAll('.image-upload-input').forEach(function(input) {
        input.addEventListener('change', function() {
            const preview = document.querySelector(this.getAttribute('data-preview'));
            if (preview && this.files && this.files[0]) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    preview.innerHTML = `<img src="${e.target.result}" alt="Preview">`;
                };
                reader.readAsDataURL(this.files[0]);
            }
        });
    });
    
    document.querySelectorAll('.image-upload-preview').forEach(function(preview) {
        preview.addEventListener('click', function() {
            const input = document.querySelector(this.getAttribute('data-input'));
            if (input) {
                input.click();
            }
        });
    });
}

function initFormValidation() {
    document.querySelectorAll('form.needs-validation').forEach(function(form) {
        form.addEventListener('submit', function(e) {
            if (!form.checkValidity()) {
                e.preventDefault();
                e.stopPropagation();
            }
            form.classList.add('was-validated');
        });
    });
}

function initTooltips() {
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function(tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

function showDynamicModal(title, content, footer) {
    const modal = document.getElementById('dynamicModal');
    document.getElementById('dynamicModalTitle').textContent = title;
    document.getElementById('dynamicModalBody').innerHTML = content;
    if (footer) {
        document.getElementById('dynamicModalFooter').innerHTML = footer;
    }
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();
}

function showImagePreview(src) {
    const modal = document.getElementById('imagePreviewModal');
    document.getElementById('previewImage').src = src;
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD'
    }).format(amount);
}

function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

function formatDateTime(dateString) {
    return new Date(dateString).toLocaleString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function startCountdown(elementId, expirationTime) {
    const element = document.getElementById(elementId);
    if (!element) return;
    
    const expiration = new Date(expirationTime).getTime();
    
    const timer = setInterval(function() {
        const now = new Date().getTime();
        const distance = expiration - now;
        
        if (distance < 0) {
            clearInterval(timer);
            element.textContent = 'Expired';
            element.classList.add('text-danger');
            return;
        }
        
        const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((distance % (1000 * 60)) / 1000);
        
        element.textContent = `${minutes}m ${seconds}s`;
    }, 1000);
}

function generateSeatGrid(rows, cols, containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    container.innerHTML = '';
    container.style.gridTemplateColumns = `repeat(${cols}, 36px)`;
    
    for (let row = 1; row <= rows; row++) {
        for (let col = 1; col <= cols; col++) {
            const seat = document.createElement('div');
            seat.className = 'seat available';
            seat.textContent = `${String.fromCharCode(64 + row)}${col}`;
            seat.setAttribute('data-row', row);
            seat.setAttribute('data-col', col);
            seat.addEventListener('click', function() {
                this.classList.toggle('selected');
            });
            container.appendChild(seat);
        }
    }
}

function showLoading() {
    let overlay = document.querySelector('.loading-overlay');
    if (!overlay) {
        overlay = document.createElement('div');
        overlay.className = 'loading-overlay';
        overlay.innerHTML = '<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>';
        document.body.appendChild(overlay);
    }
    overlay.style.display = 'flex';
}

function hideLoading() {
    const overlay = document.querySelector('.loading-overlay');
    if (overlay) {
        overlay.style.display = 'none';
    }
}

window.adminUtils = {
    showDynamicModal,
    showImagePreview,
    formatCurrency,
    formatDate,
    formatDateTime,
    startCountdown,
    generateSeatGrid,
    showLoading,
    hideLoading
};
