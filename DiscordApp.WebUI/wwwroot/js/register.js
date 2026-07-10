document.addEventListener('DOMContentLoaded', function () {
    const registerForm = document.getElementById('registerForm');

    if (registerForm) {
        registerForm.addEventListener('submit', function (e) {
            let isValid = true;

            // Elementleri Seç
            const nameInput = document.getElementById('regNameInput');
            const emailInput = document.getElementById('regEmailInput');
            const passInput = document.getElementById('regPassInput');

            const nameGroup = document.getElementById('group-regName');
            const emailGroup = document.getElementById('group-regEmail');
            const passGroup = document.getElementById('group-regPassword');

            const nameErrList = document.getElementById('err-regName');
            const emailErrList = document.getElementById('err-regEmail');
            const passErrList = document.getElementById('err-regPassword');

            // Hataları Sıfırla (Her submit denemesinde)
            nameGroup.classList.remove('has-error');
            emailGroup.classList.remove('has-error');
            passGroup.classList.remove('has-error');
            nameErrList.innerHTML = '';
            emailErrList.innerHTML = '';
            passErrList.innerHTML = '';

            // 1. Rumuz Validasyonu
            const nameVal = nameInput.value.trim();
            if (!nameVal) {
                showError(nameGroup, nameErrList, "Rumuz alanı boş bırakılamaz.");
                isValid = false;
            } else if (nameVal.length < 3) {
                showError(nameGroup, nameErrList, "Rumuz en az 3 karakter olmalıdır.");
                isValid = false;
            }

            // 2. E-posta Validasyonu
            const emailVal = emailInput.value.trim();
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

            if (!emailVal) {
                showError(emailGroup, emailErrList, "E-posta alanı boş bırakılamaz.");
                isValid = false;
            } else if (!emailRegex.test(emailVal)) {
                showError(emailGroup, emailErrList, "Lütfen geçerli bir e-posta adresi girin.");
                isValid = false;
            }

            // 3. Şifre Karmaşıklık Validasyonu (C# Modelinin Aynısı)
            const passVal = passInput.value;

            if (!passVal) {
                showError(passGroup, passErrList, "Parola alanı boş bırakılamaz.");
                isValid = false;
            } else {
                // Uzunluk Kontrolü
                if (passVal.length < 6) {
                    showError(passGroup, passErrList, "Şifre en az 6 karakter olmalıdır.");
                    isValid = false;
                }
                // Büyük Harf Kontrolü
                if (!/[A-Z]/.test(passVal)) {
                    showError(passGroup, passErrList, "Şifre en az bir büyük harf içermelidir!");
                    isValid = false;
                }
                // Küçük Harf Kontrolü
                if (!/[a-z]/.test(passVal)) {
                    showError(passGroup, passErrList, "Şifre en az bir küçük harf içermelidir!");
                    isValid = false;
                }
                // Rakam Kontrolü
                if (!/[0-9]/.test(passVal)) {
                    showError(passGroup, passErrList, "Şifre en az bir rakam (sayı) içermelidir!");
                    isValid = false;
                }
            }

            // Hata varsa formun gönderilmesini durdur
            if (!isValid) {
                e.preventDefault();
            }
        });
    }

    // Ortak hata gösterme fonksiyonu
    function showError(groupElement, listElement, message) {
        groupElement.classList.add('has-error');
        const li = document.createElement('li');
        li.textContent = message;
        listElement.appendChild(li);
    }
    const emailInput = document.getElementById('regEmailInput');

    if (emailInput) {
        // Kullanıcı e-posta kutusuna her bir harf girdiğinde veya sildiğinde tetiklenir
        emailInput.addEventListener('input', function () {
            // Sayfadaki o kırmızı sunucu hata kutusunu seçiyoruz
            const systemErrorBox = document.querySelector('.system-error-box');

            if (systemErrorBox) {
                // Kullanıcı metni değiştirmeye başladığı an kutuyu tamamen gizle
                systemErrorBox.style.display = 'none';
            }
        });
    }
});