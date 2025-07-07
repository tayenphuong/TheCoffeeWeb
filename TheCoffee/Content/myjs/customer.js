// Shopping cart functionality
//let cart = [];
//let cartCount = 3;

//function addToCart(name, price) {
//    cart.push({ name, price });
//    cartCount++;
//    document.getElementById('cartBadge').textContent = cartCount;

//    // Show notification
//    showNotification('Đã thêm ' + name + ' vào giỏ hàng!', 'success');
//}
function addToCart(productId) {
    fetch('/Cart/Add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
        },
        body: 'productId=' + productId
    })
        .then(response => {
            if (!response.ok) throw new Error("Thêm thất bại");
            return response.json();
        })
        .then(data => {
            updateCartBadge(); // cập nhật lại số lượng
            showNotification("Đã thêm vào giỏ hàng", "success");
        })
        .catch(error => {
            console.error(error);
            showNotification("Lỗi khi thêm vào giỏ hàng", "danger");
        });
}
function updateCartBadge() {
    fetch('/Cart/GetCartCount')
        .then(res => res.json())
        .then(data => {
            const badge = document.getElementById("cartBadge");
            if (badge) {
                badge.textContent = data.count;
                badge.style.display = data.count > 0 ? "inline-block" : "none";
            }
        })
        .catch(err => console.error("Lỗi khi cập nhật số lượng giỏ hàng:", err));
}



function showCart() {
    window.location.href = '/Cart/Index'; // hoặc link đến trang giỏ hàng
}
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.quantity-input').forEach(input => {
        input.addEventListener('change', function () {
            const productId = this.getAttribute('data-product-id');
            const quantity = parseInt(this.value);

            if (quantity > 0) {
                updateQuantityAjax(productId, quantity);
            }
        });
    });
});

// Menu filtering
function filterMenu(category) {
    const menuItems = document.querySelectorAll('.menu-item');
    const categoryButtons = document.querySelectorAll('.category-btn');

    // Update active button
    categoryButtons.forEach(function (btn) {
        btn.classList.remove('active', 'btn-warning');
        btn.classList.add('btn-outline-warning');
    });

    event.target.classList.add('active', 'btn-warning');
    event.target.classList.remove('btn-outline-warning');

    // Filter items
    menuItems.forEach(function (item) {
        const itemCategory = item.getAttribute('data-category');
        if (category === 'all' || itemCategory === category) {
            item.style.display = 'block';
        } else {
            item.style.display = 'none';
        }
    });
}

// Contact form
function submitForm(event) {
    event.preventDefault();
    const formData = new FormData(event.target);
    const data = {};

    for (let [key, value] of formData.entries()) {
        data[key] = value;
    }

    // Simulate form submission
    showNotification('Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.', 'success');
    event.target.reset();
}

function showMore() {
    alert('Thông tin chi tiết về Coffee House sẽ được hiển thị ở đây!');
}

// Notification system
function showNotification(message, type) {
    const notification = document.createElement('div');
    notification.className = 'alert alert-' + (type === 'success' ? 'success' : 'info') + ' alert-dismissible fade show position-fixed';
    notification.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
    notification.innerHTML = message + '<button type="button" class="btn-close" data-bs-dismiss="alert"></button>';

    document.body.appendChild(notification);

    setTimeout(function () {
        if (notification.parentNode) {
            notification.remove();
        }
    }, 5000);
}

// Smooth scrolling for anchor links
document.querySelectorAll('a[href^="#"]').forEach(function (anchor) {
    anchor.addEventListener('click', function (e) {
        const href = this.getAttribute('href');
        const target = document.querySelector(href);

        if (href && href !== "#" && target) {
            e.preventDefault();
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});


