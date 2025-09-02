window.addEventListener('DOMContentLoaded', event => {
    // Simple-DataTables
    // https://github.com/fiduswriter/Simple-DataTables/wiki

    const datatablesSimple = document.getElementById('datatablesSimple');

    if (datatablesSimple) {
        const cong = {

        };
        console.log(datatablesSimple)
        new simpleDatatables.DataTable(datatablesSimple,{
            labels: {
                    placeholder:"جستجو...",
                    perPage:"{select} آیتم در هرصفحه",
                    noRows:"هیچ آیتمی یافت نشد",
                    noResults:"هیچ آیتمی با مقدار جستجو یافت نشد",
                    info:" {start} تا {end} از {rows} آیتم"},
            layout:{top:"{select}{search}",bottom:"{info}{pager}"},
            perPage:5
        });
    }
});
