document.addEventListener('DOMContentLoaded', function () {
    const fileInput = document.getElementById('photosInput');
    const galleryContainer = document.getElementById('galleryContainer');
    const mainWrapper = document.getElementById('mainSwiperWrapper');
    const thumbWrapper = document.getElementById('thumbSwiperWrapper');
    
    // --- FUNKCJA STARTUJĄCA SWIPERA ---
    function initSwiper() {
        // Zniszcz stare instancje jeśli istnieją (dla bezpieczeństwa przy re-uploadzie)
        const oldThumb = document.querySelector(".mySwiper").swiper;
        if(oldThumb) oldThumb.destroy();
        const oldMain = document.querySelector(".mySwiper2").swiper;
        if(oldMain) oldMain.destroy();

        new Swiper(".mySwiper", {
            spaceBetween: 10,
            slidesPerView: 6,
            freeMode: true,
            watchSlidesProgress: true,
            breakpoints: {
                0: { slidesPerView: 3 },
                576: { slidesPerView: 4 },
                992: { slidesPerView: 6 }
            }
        });

        const newThumbSwiper = document.querySelector(".mySwiper").swiper;

        new Swiper(".mySwiper2", {
            spaceBetween: 10,
            navigation: { nextEl: ".swiper-button-next", prevEl: ".swiper-button-prev" },
            thumbs: { swiper: newThumbSwiper }
        });
    }

    if (!galleryContainer.classList.contains('d-none')) {
        setTimeout(initSwiper, 100);
    }


    if (fileInput) {
        fileInput.addEventListener('change', function () {

            mainWrapper.innerHTML = '';
            thumbWrapper.innerHTML = '';
            
            const files = Array.from(this.files);
            if (files.length === 0) return; 

            galleryContainer.classList.remove('d-none');

            let loadedCount = 0;
            files.forEach((file) => {
                if (!file.type.startsWith('image/')) return;
                const reader = new FileReader();
                reader.onload = function (e) {
                    mainWrapper.innerHTML += `<div class="swiper-slide"><img src="${e.target.result}" /></div>`;
                    thumbWrapper.innerHTML += `<div class="swiper-slide border border-secondary"><img src="${e.target.result}" style="object-fit: contain;" /></div>`;
                    
                    loadedCount++;
                    if(loadedCount === files.length) {
                        setTimeout(initSwiper, 100);
                    }
                };
                reader.readAsDataURL(file);
            });
        });
    }
});