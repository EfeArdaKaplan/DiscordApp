// Ortak yardımcı fonksiyonlar - hem Login, hem Register, hem de App sayfasında kullanılır.

function initials(name) {
  return name.split(" ").map(w => w[0]).join("").slice(0, 2).toUpperCase();
}

function isEmpty(v) {
  return v === undefined || v === null || v.trim() === '';
}

function isValidEmail(v) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v);
}

function renderErrors(prefix, errors) {
  Object.keys(errors).forEach(key => {
    const list = document.getElementById('err-' + prefix + key);
    const group = document.getElementById('group-' + prefix + key);
    if (!list || !group) return;
    list.innerHTML = errors[key].map(msg => `<li>${msg}</li>`).join('');
    group.classList.toggle('has-error', errors[key].length > 0);
  });
}
