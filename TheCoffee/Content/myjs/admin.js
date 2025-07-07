
// Sidebar Toggle
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('mainContent');

    sidebar.classList.toggle('collapsed');
    mainContent.classList.toggle('expanded');
}

// Navigation
function showSection(sectionName) {
    // Hide all sections
    document.querySelectorAll('.content-section').forEach(section => {
        section.classList.add('d-none');
    });

    // Show selected section
    document.getElementById(sectionName + '-section').classList.remove('d-none');

    // Update page title
    const titles = {
        'dashboard': 'Dashboard',
        'products': 'Quản lý sản phẩm',
        'orders': 'Quản lý đơn hàng',
        'customers': 'Quản lý khách hàng',
        'categories': 'Quản lý danh mục',
        'reports': 'Báo cáo',
        'settings': 'Cài đặt'
    };
    document.getElementById('pageTitle').textContent = titles[sectionName];

    // Update active menu
    document.querySelectorAll('.menu-link').forEach(link => {
        link.classList.remove('active');
    });
    document.querySelector(`[data-section="${sectionName}"]`).classList.add('active');
}

// Menu click handlers
document.querySelectorAll('.menu-link').forEach(link => {
    link.addEventListener('click', function (e) {
        e.preventDefault();
        const section = this.getAttribute('data-section');
        showSection(section);
    });
});

// Charts
function initCharts() {
    // Revenue Chart
    const revenueCtx = document.getElementById('revenueChart').getContext('2d');
    new Chart(revenueCtx, {
        type: 'line',
        data: {
            labels: ['T1', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'T8', 'T9', 'T10', 'T11', 'T12'],
            datasets: [{
                label: 'Doanh thu (triệu VND)',
                data: [65, 78, 90, 81, 95, 105, 110, 125, 115, 130, 140, 125],
                borderColor: '#ffc107',
                backgroundColor: 'rgba(255, 193, 7, 0.1)',
                tension: 0.4,
                fill: true
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });

    // Product Chart
    const productCtx = document.getElementById('productChart').getContext('2d');
    new Chart(productCtx, {
        type: 'doughnut',
        data: {
            labels: ['Cappuccino', 'Latte', 'Espresso', 'Americano', 'Khác'],
            datasets: [{
                data: [30, 25, 20, 15, 10],
                backgroundColor: [
                    '#ffc107',
                    '#28a745',
                    '#007bff',
                    '#dc3545',
                    '#6c757d'
                ]
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}

// Product Management
function editProduct(id) {
    // Load product data and show modal
    document.getElementById('productModal').querySelector('.modal-title').textContent = 'Chỉnh sửa sản phẩm';
    const modal = new bootstrap.Modal(document.getElementById('productModal'));
    modal.show();
}

function deleteProduct(id) {
    if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này?')) {
        // Delete product logic
        showNotification('Đã xóa sản phẩm thành công!', 'success');
    }
}

function saveProduct() {
    const form = document.getElementById('productForm');
    const formData = new FormData(form);

    // Show loading
    const button = event.target;
    const loading = button.querySelector('.loading');
    loading.classList.remove('d-none');
    button.disabled = true;

    // Simulate API call
    setTimeout(() => {
        loading.classList.add('d-none');
        button.disabled = false;

        // Close modal and show success
        bootstrap.Modal.getInstance(document.getElementById('productModal')).hide();
        showNotification('Đã lưu sản phẩm thành công!', 'success');

        // Reset form
        form.reset();
        document.getElementById('imagePreview').classList.add('d-none');
    }, 2000);
}

function previewImage(input) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            const preview = document.getElementById('imagePreview');
            preview.src = e.target.result;
            preview.classList.remove('d-none');
        };
        reader.readAsDataURL(input.files[0]);
    }
}

// Order Management
function viewOrder(orderId) {
    alert('Xem chi tiết đơn hàng #' + orderId);
}

function updateOrderStatus(orderId, status) {
    const statusText = {
        'completed': 'hoàn thành',
        'cancelled': 'hủy'
    };

    if (confirm(`Bạn có chắc chắn muốn ${statusText[status]} đơn hàng #${orderId}?`)) {
        showNotification(`Đã ${statusText[status]} đơn hàng #${orderId}!`, 'success');
    }
}

// Filters
function resetFilters() {
    document.getElementById('searchProduct').value = '';
    document.getElementById('categoryFilter').value = '';
    document.getElementById('statusFilter').value = '';
}

// Notification System
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type === 'success' ? 'success' : 'info'} alert-dismissible fade show position-fixed`;
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = `
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            `;

    document.body.appendChild(notification);

    setTimeout(() => {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}

// Initialize
document.addEventListener('DOMContentLoaded', function () {
    initCharts();
    console.log('Coffee House Admin Dashboard loaded successfully!');
});

// Responsive sidebar for mobile
if (window.innerWidth <= 768) {
    document.getElementById('sidebar').classList.add('collapsed');
    document.getElementById('mainContent').classList.add('expanded');
}