// TPL Web Application - Vendor JavaScript Bundle
// This file imports all third-party libraries and dependencies

// jQuery
import 'jquery';

// Bootstrap
import 'bootstrap/dist/js/bootstrap.bundle.js';

// Perfect Scrollbar
import 'perfect-scrollbar';

// Quill Editor
import 'quill';

// SweetAlert2
import 'sweetalert2';

// DataTables
import 'datatables.net';
import 'datatables.net-bs5';
import 'datatables.net-responsive';
import 'datatables.net-responsive-bs5';

// Flatpickr
import 'flatpickr';

// Select2
import 'select2';

// Dropzone
import 'dropzone';

// Make libraries available globally
window.$ = window.jQuery = $;
window.bootstrap = bootstrap;
window.PerfectScrollbar = PerfectScrollbar;
window.Quill = Quill;
window.Swal = Swal;
window.DataTable = DataTable;
window.flatpickr = flatpickr;
window.Select2 = Select2;
window.Dropzone = Dropzone;

console.log('Vendor libraries loaded successfully');
