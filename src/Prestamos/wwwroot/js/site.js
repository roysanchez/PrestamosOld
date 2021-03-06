//TODO Ver como se puede manejar el ambiente de producción
var require = {
    baseUrl: '/js',
    paths: {
        lib: '../lib',
        libext: '../libext',
        bootstrap: '../lib/bootstrap/dist/js/umd',
        jquery: '../lib/jquery/dist/jquery',
        picker: '../lib/pickadate/lib/picker',
        pickadate: '../lib/pickadate/lib/picker.date',
        'jquery.validate': '../lib/jquery-validation/dist/jquery.validate',
        'jquery.validate.unobtrusive': '../lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive',
        'jquery.validation.unobtrusive.bootstrap': '../libext/jquery-validation-unobtrusive-bootstrap/jquery.validation.unobtrusive.bootstrap'
    },
    map:{
        '*':{
            'Modernizr': 'libext/modernizr/modernizr-input',
            'vuejs': 'lib/vue/dist/vue',
            'jquery': 'Comun/jquery-private'
        },
        'Comun/jquery-private': { 'jquery': 'jquery' }
    },
    shim: {
        'libext/modernizr/modernizr-input': {
            exports: 'Modernizr',
            init: function () {
                var ret = this.Modernizr;
                this.Modernizr = undefined;
                return ret;
            }
        }
    },
    enforceDefine: true
};