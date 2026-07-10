document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.getElementById('loginForm');

    if (loginForm) {
        loginForm.addEventListener('submit', function (e) {
            let isValid = true;

            // Elementleri Seç
            const emailInput = document.getElementById('loginEmailInput');
            const passInput = document.getElementById('loginPassInput');
            const emailGroup = document.getElementById('group-loginEmail');
            const passGroup = document.getElementById('group-loginPassword');
            const emailErrList = document.getElementById('err-loginEmail');
            const passErrList = document.getElementById('err-loginPassword');

            // Hataları Sıfırla (Her submit denemesinde temizle)
            emailGroup.classList.remove('has-error');
            passGroup.classList.remove('has-error');
            emailErrList.innerHTML = '';
            passErrList.innerHTML = '';

            // E-posta Validasyonu
            const emailVal = emailInput.value.trim();
            const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

            if (!emailVal) {
                showError(emailGroup, emailErrList, "E-posta alanı boş bırakılamaz.");
                isValid = false;
            } else if (!emailRegex.test(emailVal)) {
                showError(emailGroup, emailErrList, "Lütfen geçerli bir e-posta adresi girin.");
                isValid = false;
            }

            // Şifre Validasyonu
            const passVal = passInput.value;

            if (!passVal) {
                showError(passGroup, passErrList, "Parola alanı boş bırakılamaz.");
                isValid = false;
            } else if (passVal.length < 6) {
                showError(passGroup, passErrList, "Parolanız en az 6 karakter olmalıdır.");
                isValid = false;
            }

            // Eğer hata varsa formun sunucuya gitmesini engelle
            if (!isValid) {
                e.preventDefault();
            }
        });
    }

    // Hata gösterme yardımcı fonksiyonu
    function showError(groupElement, listElement, message) {
        groupElement.classList.add('has-error');
        const li = document.createElement('li');
        li.textContent = message;
        listElement.appendChild(li);
    }
});