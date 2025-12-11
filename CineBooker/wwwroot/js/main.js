/* ========================================
   Initialization
   ======================================== */
document.addEventListener('DOMContentLoaded', function () {
    // Initialize components based on current page
    initializeRatingStars();
    initializeDateButtons();
    initializeFormValidation();

    // Remove loading spinner if present
    setTimeout(hideLoading, 500);

    // Log initialization
    console.log('CineMax Application Initialized');
});

// Handle page visibility for session timer
document.addEventListener('visibilitychange', function () {
    if (document.hidden) {
        console.log('Page hidden - session timer paused');
    } else {
        console.log('Page visible - session timer resumed');
    }
});================================
    CineMax - Cinema Ticket Booking Application
   JavaScript Functions for ASP.NET MVC Front - End
    ======================================== * /

// @* For ASP.NET MVC: This JavaScript works alongside Bootstrap 5 *@

/* ========================================
   Global Variables
   ======================================== */
const REGULAR_SEAT_PRICE = 15.00;
const PREMIUM_SEAT_PRICE = 25.00;
const CONVENIENCE_FEE_PER_TICKET = 1.50;
const IMAX_SURCHARGE = 3.00;

let selectedSeats = [];
let bookedSeats = [];
let sessionTimeRemaining = 600; // 10 minutes in seconds

/* ========================================
   Utility Functions
   ======================================== */

// Show loading spinner
function showLoading() {
    const spinner = document.getElementById('loadingSpinner');
    if (spinner) {
        spinner.classList.remove('d-none');
    }
}

// Hide loading spinner
function hideLoading() {
    const spinner = document.getElementById('loadingSpinner');
    if (spinner) {
        spinner.classList.add('d-none');
    }
}

// Show toast notification
function showToast(title, message, type = 'info') {
    const toastEl = document.getElementById('notificationToast');
    const toastTitle = document.getElementById('toastTitle');
    const toastMessage = document.getElementById('toastMessage');

    if (toastEl && toastTitle && toastMessage) {
        toastTitle.textContent = title;
        toastMessage.textContent = message;

        // Update icon color based on type
        const icon = toastEl.querySelector('.toast-header i');
        if (icon) {
            icon.className = 'bi me-2';
            switch (type) {
                case 'success':
                    icon.classList.add('bi-check-circle-fill', 'text-success');
                    break;
                case 'error':
                    icon.classList.add('bi-x-circle-fill', 'text-danger');
                    break;
                case 'warning':
                    icon.classList.add('bi-exclamation-triangle-fill', 'text-warning');
                    break;
                default:
                    icon.classList.add('bi-bell-fill', 'text-danger');
            }
        }

        const toast = new bootstrap.Toast(toastEl);
        toast.show();
    }
}

// Format currency
function formatCurrency(amount) {
    return '$' + amount.toFixed(2);
}

// Store data in localStorage
function storeBookingData(key, data) {
    try {
        localStorage.setItem(key, JSON.stringify(data));
    } catch (e) {
        console.error('Error storing data:', e);
    }
}

// Retrieve data from localStorage
function getBookingData(key) {
    try {
        const data = localStorage.getItem(key);
        return data ? JSON.parse(data) : null;
    } catch (e) {
        console.error('Error retrieving data:', e);
        return null;
    }
}

/* ========================================
   City Search/Filter Functions
   ======================================== */
function filterCities() {
    const searchInput = document.getElementById('citySearch');
    if (!searchInput) return;

    const searchTerm = searchInput.value.toLowerCase();
    const cityCards = document.querySelectorAll('.city-card');
    const noResults = document.getElementById('noResults');
    const cityCount = document.getElementById('cityCount');
    let visibleCount = 0;

    cityCards.forEach(card => {
        const cityName = card.getAttribute('data-city') || '';
        if (cityName.includes(searchTerm)) {
            card.style.display = 'block';
            visibleCount++;
        } else {
            card.style.display = 'none';
        }
    });

    // Update count and no results message
    if (cityCount) {
        cityCount.textContent = visibleCount + ' Cities';
    }

    if (noResults) {
        noResults.classList.toggle('d-none', visibleCount > 0);
    }
}

/* ========================================
   Cinema Search/Filter Functions
   ======================================== */
function filterCinemas() {
    const searchInput = document.getElementById('cinemaSearch');
    if (!searchInput) return;

    const searchTerm = searchInput.value.toLowerCase();
    const cinemaCards = document.querySelectorAll('.cinema-card');
    const noResults = document.getElementById('noResults');
    const cinemaCount = document.getElementById('cinemaCount');
    let visibleCount = 0;

    cinemaCards.forEach(card => {
        const cinemaName = card.getAttribute('data-name') || '';
        if (cinemaName.includes(searchTerm)) {
            card.style.display = 'block';
            visibleCount++;
        } else {
            card.style.display = 'none';
        }
    });

    if (cinemaCount) {
        cinemaCount.textContent = visibleCount + ' Cinemas Found';
    }

    if (noResults) {
        noResults.classList.toggle('d-none', visibleCount > 0);
    }
}

/* ========================================
   Movie Search/Filter Functions
   ======================================== */
function filterMovies() {
    const searchInput = document.getElementById('movieSearch');
    const genreFilter = document.getElementById('genreFilter');
    const languageFilter = document.getElementById('languageFilter');
    const dateFilter = document.getElementById('dateFilter');

    const searchTerm = searchInput ? searchInput.value.toLowerCase() : '';
    const selectedGenre = genreFilter ? genreFilter.value.toLowerCase() : '';
    const selectedLanguage = languageFilter ? languageFilter.value.toLowerCase() : '';
    const selectedDate = dateFilter ? dateFilter.value : '';

    const movieCards = document.querySelectorAll('.movie-card');
    const noResults = document.getElementById('noResults');
    const movieCount = document.getElementById('movieCount');
    let visibleCount = 0;

    movieCards.forEach(card => {
        const title = (card.getAttribute('data-title') || '').toLowerCase();
        const genre = (card.getAttribute('data-genre') || '').toLowerCase();
        const language = (card.getAttribute('data-language') || '').toLowerCase();
        const date = card.getAttribute('data-date') || '';

        const matchesSearch = !searchTerm || title.includes(searchTerm);
        const matchesGenre = !selectedGenre || genre.includes(selectedGenre);
        const matchesLanguage = !selectedLanguage || language.includes(selectedLanguage);
        const matchesDate = !selectedDate || date === selectedDate;

        if (matchesSearch && matchesGenre && matchesLanguage && matchesDate) {
            card.style.display = 'block';
            visibleCount++;
        } else {
            card.style.display = 'none';
        }
    });

    if (movieCount) {
        movieCount.textContent = visibleCount + ' Movies';
    }

    if (noResults) {
        noResults.classList.toggle('d-none', visibleCount > 0);
    }
}

function clearFilters() {
    const searchInput = document.getElementById('movieSearch');
    const genreFilter = document.getElementById('genreFilter');
    const languageFilter = document.getElementById('languageFilter');
    const dateFilter = document.getElementById('dateFilter');

    if (searchInput) searchInput.value = '';
    if (genreFilter) genreFilter.value = '';
    if (languageFilter) languageFilter.value = '';
    if (dateFilter) dateFilter.value = '';

    filterMovies();
    showToast('Filters Cleared', 'All filters have been reset', 'info');
}

/* ========================================
   Seat Selection Functions
   ======================================== */
function initializeSeats() {
    const seatsContainer = document.getElementById('seatsContainer');
    if (!seatsContainer) return;

    // Simulate some booked seats
    bookedSeats = ['A3', 'A4', 'B7', 'C5', 'C6', 'D10', 'E8', 'F3', 'F4', 'F5', 'G9', 'H2', 'H3'];

    const rows = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J'];
    const seatsPerRow = 14;
    const premiumRows = ['A', 'B', 'C'];

    seatsContainer.innerHTML = '';

    rows.forEach(row => {
        const rowDiv = document.createElement('div');
        rowDiv.className = 'seat-row';

        // Row label (left)
        const leftLabel = document.createElement('span');
        leftLabel.className = 'row-label';
        leftLabel.textContent = row;
        rowDiv.appendChild(leftLabel);

        // Create seats
        for (let i = 1; i <= seatsPerRow; i++) {
            // Add gap in the middle (aisle)
            if (i === 4 || i === 12) {
                const gap = document.createElement('div');
                gap.className = 'seat-gap';
                rowDiv.appendChild(gap);
            }

            const seatId = row + i;
            const seat = document.createElement('button');
            seat.className = 'seat';
            seat.setAttribute('data-seat', seatId);
            seat.textContent = i;

            const isPremium = premiumRows.includes(row);
            const isBooked = bookedSeats.includes(seatId);

            if (isBooked) {
                seat.classList.add('booked');
                seat.disabled = true;
            } else if (isPremium) {
                seat.classList.add('premium', 'available');
            } else {
                seat.classList.add('available');
            }

            seat.addEventListener('click', () => toggleSeat(seatId, isPremium));
            rowDiv.appendChild(seat);
        }

        // Row label (right)
        const rightLabel = document.createElement('span');
        rightLabel.className = 'row-label';
        rightLabel.textContent = row;
        rowDiv.appendChild(rightLabel);

        seatsContainer.appendChild(rowDiv);
    });

    updateBookingSummary();
}

function toggleSeat(seatId, isPremium) {
    const seatElement = document.querySelector(`[data-seat="${seatId}"]`);
    if (!seatElement || seatElement.classList.contains('booked')) return;

    // Check for double booking
    const existingBookings = getBookingData('cinemax_bookings') || [];
    const isDoubleBooked = existingBookings.some(booking =>
        booking.seats && booking.seats.includes(seatId)
    );

    if (isDoubleBooked) {
        showToast('Already Booked', 'This seat has already been booked by you.', 'warning');
        return;
    }

    const seatIndex = selectedSeats.findIndex(s => s.id === seatId);

    if (seatIndex > -1) {
        // Deselect seat
        selectedSeats.splice(seatIndex, 1);
        seatElement.classList.remove('selected');
        seatElement.classList.add('available');
        if (isPremium) seatElement.classList.add('premium');
    } else {
        // Select seat (max 10 seats)
        if (selectedSeats.length >= 10) {
            showToast('Maximum Seats', 'You can only select up to 10 seats.', 'warning');
            return;
        }

        selectedSeats.push({
            id: seatId,
            isPremium: isPremium
        });
        seatElement.classList.remove('available', 'premium');
        seatElement.classList.add('selected');
    }

    updateBookingSummary();
}

function updateBookingSummary() {
    const seatCountEl = document.getElementById('seatCount');
    const selectedSeatsListEl = document.getElementById('selectedSeatsList');
    const regularCountEl = document.getElementById('regularCount');
    const premiumCountEl = document.getElementById('premiumCount');
    const regularPriceEl = document.getElementById('regularPrice');
    const premiumPriceEl = document.getElementById('premiumPrice');
    const convenienceFeeEl = document.getElementById('convenienceFee');
    const totalAmountEl = document.getElementById('totalAmount');
    const proceedBtn = document.getElementById('proceedBtn');

    const regularSeats = selectedSeats.filter(s => !s.isPremium);
    const premiumSeats = selectedSeats.filter(s => s.isPremium);

    const regularTotal = regularSeats.length * REGULAR_SEAT_PRICE;
    const premiumTotal = premiumSeats.length * PREMIUM_SEAT_PRICE;
    const imaxTotal = selectedSeats.length * IMAX_SURCHARGE;
    const convenienceFee = selectedSeats.length * CONVENIENCE_FEE_PER_TICKET;
    const total = regularTotal + premiumTotal + imaxTotal + convenienceFee;

    // Update UI
    if (seatCountEl) seatCountEl.textContent = selectedSeats.length;
    if (regularCountEl) regularCountEl.textContent = regularSeats.length;
    if (premiumCountEl) premiumCountEl.textContent = premiumSeats.length;
    if (regularPriceEl) regularPriceEl.textContent = formatCurrency(regularTotal + (regularSeats.length * IMAX_SURCHARGE));
    if (premiumPriceEl) premiumPriceEl.textContent = formatCurrency(premiumTotal + (premiumSeats.length * IMAX_SURCHARGE));
    if (convenienceFeeEl) convenienceFeeEl.textContent = formatCurrency(convenienceFee);
    if (totalAmountEl) totalAmountEl.textContent = formatCurrency(total);

    // Update selected seats list
    if (selectedSeatsListEl) {
        if (selectedSeats.length === 0) {
            selectedSeatsListEl.innerHTML = '<p class="text-secondary small mb-0">No seats selected</p>';
        } else {
            selectedSeatsListEl.innerHTML = selectedSeats.map(s =>
                `<span class="selected-seat-badge">${s.id}${s.isPremium ? ' (P)' : ''}</span>`
            ).join('');
        }
    }

    // Enable/disable proceed button
    if (proceedBtn) {
        proceedBtn.disabled = selectedSeats.length === 0;
    }

    // Store selected seats
    storeBookingData('selected_seats', selectedSeats);
}

function proceedToPayment() {
    if (selectedSeats.length === 0) {
        showToast('No Seats Selected', 'Please select at least one seat to proceed.', 'warning');
        return;
    }

    showLoading();

    // Store booking data
    const bookingData = {
        seats: selectedSeats,
        movie: 'The Dark Horizon',
        cinema: 'CineMax Times Square',
        hall: 'Hall 1 - IMAX',
        date: 'Nov 28, 2024',
        time: '7:00 PM',
        total: calculateTotal()
    };

    storeBookingData('current_booking', bookingData);

    // Simulate loading and redirect
    setTimeout(() => {
        window.location.href = 'Payment.html';
    }, 1000);
}

function calculateTotal() {
    const regularSeats = selectedSeats.filter(s => !s.isPremium);
    const premiumSeats = selectedSeats.filter(s => s.isPremium);

    const regularTotal = regularSeats.length * (REGULAR_SEAT_PRICE + IMAX_SURCHARGE);
    const premiumTotal = premiumSeats.length * (PREMIUM_SEAT_PRICE + IMAX_SURCHARGE);
    const convenienceFee = selectedSeats.length * CONVENIENCE_FEE_PER_TICKET;

    return regularTotal + premiumTotal + convenienceFee;
}

/* ========================================
   Payment Functions
   ======================================== */
function applyCoupon() {
    const couponInput = document.getElementById('couponCode');
    const couponMessage = document.getElementById('couponMessage');
    const discountRow = document.getElementById('discountRow');
    const discountAmount = document.getElementById('discountAmount');
    const paymentTotal = document.getElementById('paymentTotal');

    if (!couponInput) return;

    const code = couponInput.value.toUpperCase().trim();
    let discount = 0;
    let message = '';
    let isValid = false;

    // Validate coupon codes
    switch (code) {
        case 'FIRST10':
            discount = 56.50 * 0.10; // 10% off
            message = 'Coupon applied! You got 10% off.';
            isValid = true;
            break;
        case 'MOVIE20':
            discount = 20.00; // $20 off
            message = 'Coupon applied! You got $20 off.';
            isValid = true;
            break;
        case 'WEEKEND':
            discount = 56.50 * 0.15; // 15% off
            message = 'Coupon applied! You got 15% off.';
            isValid = true;
            break;
        default:
            message = 'Invalid coupon code. Please try again.';
            isValid = false;
    }

    if (couponMessage) {
        couponMessage.innerHTML = `<small class="${isValid ? 'text-success' : 'text-danger'}">${message}</small>`;
    }

    if (isValid) {
        if (discountRow) discountRow.style.display = 'flex !important';
        if (discountAmount) discountAmount.textContent = '-' + formatCurrency(discount);
        if (paymentTotal) {
            const newTotal = 56.50 - discount;
            paymentTotal.textContent = formatCurrency(newTotal);

            // Update pay button
            const payBtn = document.querySelector('.btn-danger.btn-lg');
            if (payBtn) {
                payBtn.innerHTML = `<i class="bi bi-lock-fill me-2"></i>Pay ${formatCurrency(newTotal)}`;
            }
        }
        showToast('Coupon Applied', message, 'success');
    } else {
        showToast('Invalid Coupon', message, 'error');
    }
}

function selectCoupon(code) {
    const couponInput = document.getElementById('couponCode');
    if (couponInput) {
        couponInput.value = code;
        applyCoupon();
    }
}

function processPayment() {
    const agreeTerms = document.getElementById('agreeTerms');
    const fullName = document.getElementById('fullName');
    const email = document.getElementById('email');
    const phone = document.getElementById('phone');

    // Validate terms
    if (agreeTerms && !agreeTerms.checked) {
        showToast('Terms Required', 'Please agree to the terms and conditions.', 'warning');
        return;
    }

    // Basic validation
    if (fullName && !fullName.value.trim()) {
        fullName.classList.add('is-invalid');
        showToast('Name Required', 'Please enter your full name.', 'warning');
        return;
    }

    if (email && !email.value.trim()) {
        email.classList.add('is-invalid');
        showToast('Email Required', 'Please enter your email address.', 'warning');
        return;
    }

    if (phone && !phone.value.trim()) {
        phone.classList.add('is-invalid');
        showToast('Phone Required', 'Please enter your phone number.', 'warning');
        return;
    }

    showLoading();

    // Simulate payment processing
    setTimeout(() => {
        // Store booking in localStorage to prevent double booking
        const bookingData = getBookingData('current_booking') || {};
        const existingBookings = getBookingData('cinemax_bookings') || [];

        bookingData.id = 'CMX-' + Date.now();
        bookingData.status = 'confirmed';
        bookingData.bookedAt = new Date().toISOString();

        existingBookings.push(bookingData);
        storeBookingData('cinemax_bookings', existingBookings);

        // Clear current booking
        localStorage.removeItem('current_booking');
        localStorage.removeItem('selected_seats');

        // Redirect to success page
        window.location.href = 'Success.html';
    }, 2000);
}

function startSessionTimer() {
    const timerEl = document.getElementById('sessionTimer');
    if (!timerEl) return;

    const interval = setInterval(() => {
        sessionTimeRemaining--;

        const minutes = Math.floor(sessionTimeRemaining / 60);
        const seconds = sessionTimeRemaining % 60;

        timerEl.textContent = `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;

        if (sessionTimeRemaining <= 60) {
            timerEl.classList.add('text-danger');
        }

        if (sessionTimeRemaining <= 0) {
            clearInterval(interval);
            showToast('Session Expired', 'Your session has expired. Please start again.', 'error');
            setTimeout(() => {
                window.location.href = '../Movie/List.html';
            }, 2000);
        }
    }, 1000);
}

function formatCardNumber() {
    const cardInput = document.getElementById('cardNumber');
    if (!cardInput) return;

    cardInput.addEventListener('input', function (e) {
        let value = e.target.value.replace(/\s+/g, '').replace(/[^0-9]/gi, '');
        let formattedValue = '';

        for (let i = 0; i < value.length; i++) {
            if (i > 0 && i % 4 === 0) {
                formattedValue += ' ';
            }
            formattedValue += value[i];
        }

        e.target.value = formattedValue;
    });
}

function formatExpiry() {
    const expiryInput = document.getElementById('expiry');
    if (!expiryInput) return;

    expiryInput.addEventListener('input', function (e) {
        let value = e.target.value.replace(/\D/g, '');

        if (value.length >= 2) {
            value = value.substring(0, 2) + '/' + value.substring(2);
        }

        e.target.value = value;
    });
}

/* ========================================
   Booking Cancellation Functions
   ======================================== */
function confirmCancellation() {
    const confirmCheck = document.getElementById('confirmCancel');

    if (!confirmCheck || !confirmCheck.checked) {
        showToast('Confirmation Required', 'Please confirm that you want to cancel.', 'warning');
        return;
    }

    showLoading();

    // Simulate cancellation process
    setTimeout(() => {
        hideLoading();

        // Hide cancel section and show confirmed section
        const cancelSection = document.getElementById('cancelSection');
        const cancelledSection = document.getElementById('cancelledSection');

        if (cancelSection) cancelSection.classList.add('d-none');
        if (cancelledSection) cancelledSection.classList.remove('d-none');

        showToast('Booking Cancelled', 'Your booking has been cancelled successfully.', 'success');
    }, 2000);
}

/* ========================================
   Ticket Functions
   ======================================== */
function downloadTicket() {
    showLoading();

    setTimeout(() => {
        hideLoading();
        showToast('Download Started', 'Your ticket is being downloaded...', 'success');

        // In a real app, this would generate a PDF
        // For demo, we'll just show a success message
    }, 1500);
}

function shareTicket() {
    if (navigator.share) {
        navigator.share({
            title: 'CineMax Ticket',
            text: 'Check out my movie ticket for The Dark Horizon!',
            url: window.location.href
        }).then(() => {
            showToast('Shared!', 'Ticket shared successfully.', 'success');
        }).catch(() => {
            showToast('Share Failed', 'Could not share the ticket.', 'error');
        });
    } else {
        // Fallback for browsers that don't support Web Share API
        navigator.clipboard.writeText(window.location.href).then(() => {
            showToast('Link Copied', 'Ticket link copied to clipboard!', 'success');
        });
    }
}

/* ========================================
   Rating Functions
   ======================================== */
function initializeRatingStars() {
    const stars = document.querySelectorAll('.rating-star');
    const ratingText = document.getElementById('ratingText');

    const ratingTexts = [
        'Select your rating',
        'Poor',
        'Fair',
        'Good',
        'Very Good',
        'Excellent'
    ];

    stars.forEach(star => {
        star.addEventListener('mouseover', function () {
            const rating = parseInt(this.getAttribute('data-rating'));
            highlightStars(rating);
        });

        star.addEventListener('mouseout', function () {
            const selectedRating = document.querySelector('.rating-star.selected');
            if (selectedRating) {
                highlightStars(parseInt(selectedRating.getAttribute('data-rating')));
            } else {
                highlightStars(0);
            }
        });

        star.addEventListener('click', function () {
            const rating = parseInt(this.getAttribute('data-rating'));
            stars.forEach(s => s.classList.remove('selected'));
            this.classList.add('selected');
            highlightStars(rating);
            if (ratingText) {
                ratingText.textContent = ratingTexts[rating];
            }
        });
    });
}

function highlightStars(count) {
    const stars = document.querySelectorAll('.rating-star');
    stars.forEach((star, index) => {
        if (index < count) {
            star.classList.remove('bi-star');
            star.classList.add('bi-star-fill', 'text-warning');
        } else {
            star.classList.remove('bi-star-fill', 'text-warning');
            star.classList.add('bi-star');
        }
    });
}

function submitRating() {
    const selectedStar = document.querySelector('.rating-star.selected');

    if (!selectedStar) {
        showToast('Rating Required', 'Please select a rating before submitting.', 'warning');
        return;
    }

    const rating = parseInt(selectedStar.getAttribute('data-rating'));

    showLoading();

    setTimeout(() => {
        hideLoading();

        // Close modal
        const modal = bootstrap.Modal.getInstance(document.getElementById('ratingModal'));
        if (modal) modal.hide();

        showToast('Rating Submitted', `Thank you for rating ${rating} stars!`, 'success');
    }, 1000);
}

/* ========================================
   Booking Filter Functions
   ======================================== */
function filterBookings(status) {
    const cards = document.querySelectorAll('.booking-card');
    const noBookings = document.getElementById('noBookings');
    let visibleCount = 0;

    // Update button states
    document.querySelectorAll('.btn-group .btn').forEach(btn => {
        btn.classList.remove('btn-danger', 'active');
        btn.classList.add('btn-outline-secondary');
    });

    event.target.classList.remove('btn-outline-secondary');
    event.target.classList.add('btn-danger', 'active');

    cards.forEach(card => {
        const cardStatus = card.getAttribute('data-status');

        if (status === 'all' || cardStatus === status) {
            card.style.display = 'block';
            visibleCount++;
        } else {
            card.style.display = 'none';
        }
    });

    if (noBookings) {
        noBookings.classList.toggle('d-none', visibleCount > 0);
    }
}

/* ========================================
   View Toggle Functions
   ======================================== */
function setView(viewType) {
    // Update button states
    document.querySelectorAll('.btn-group .btn').forEach(btn => {
        btn.classList.remove('active');
    });
    event.target.classList.add('active');

    // In a full implementation, this would toggle between grid and list views
    showToast('View Changed', `Switched to ${viewType} view`, 'info');
}

/* ========================================
   Date Selection Functions
   ======================================== */
function initializeDateButtons() {
    const dateButtons = document.querySelectorAll('.date-btn');

    dateButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            dateButtons.forEach(b => {
                b.classList.remove('btn-danger', 'active');
                b.classList.add('btn-outline-secondary');
            });

            this.classList.remove('btn-outline-secondary');
            this.classList.add('btn-danger', 'active');

            // In a real app, this would load shows for the selected date
            showToast('Date Selected', 'Loading shows for selected date...', 'info');
        });
    });
}

/* ========================================
   Form Validation - FIXED VERSION
   ======================================== */
function initializeFormValidation() {
    // ✅ التعديل: نطبق الـ validation فقط على الـ forms اللي عليها الـ class ده
    const forms = document.querySelectorAll('form.needs-custom-validation');

    forms.forEach(form => {
        form.addEventListener('submit', function (e) {
            // Simple validation
            const inputs = form.querySelectorAll('input[required]');
            let isValid = true;

            inputs.forEach(input => {
                if (!input.value.trim()) {
                    input.classList.add('is-invalid');
                    isValid = false;
                } else {
                    input.classList.remove('is-invalid');
                }
            });

            // ✅ التعديل: نمنع الإرسال فقط لو الـ form مش valid
            if (!isValid) {
                e.preventDefault();
                showToast('Validation Error', 'Please fill all required fields', 'warning');
            }
            // لو valid، نسيب الـ form يرسل عادي (مش بنستخدم preventDefault)
        });
    });
}