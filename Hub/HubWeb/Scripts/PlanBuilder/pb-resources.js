(function (ns) {

    // Asynchronous image loader.
    ns.ImageLoader = Core.class({
        constructor: function () {
            this._imageUrls = [
                ns.WidgetConsts.startNodeBgImage,
                ns.WidgetConsts.addCriteriaNodeBgImage,
                ns.WidgetConsts.criteriaNodeBgImage,
                ns.WidgetConsts.actionsNodeTopImage,
                ns.WidgetConsts.actionsNodeBottomImage,
                ns.WidgetConsts.actionsNodeBgImage,
                ns.WidgetConsts.addActionNodeAddImage
            ];

            this._images = {};
        },

        // Loads images asynchronously and calls callback function, when all images are loaded and cached.
        // Parameters: 
        //     callback - callback function to call.
        loadImages: function (callback) {
            var loaded = 0;
            var loadCallback = Core.delegate(
                function (imageKey, image) {
                    this._images[imageKey] = image;

                    ++loaded;
                    if (loaded === this._imageUrls.length) {
                        callback();
                    }
                },
                this
            );

            var i, image;
            for (i = 0; i < this._imageUrls.length; ++i) {
                image = new Image();
                image.src = this._imageUrls[i];

                image.onload = (function (imageKey, image) {
                    return function () { loadCallback(imageKey, image); };
                }) (this._imageUrls[i], image);
            }
        },

        // Get cached image.
        getImage: function (imageKey) {
            return this._images[imageKey];
        }
    });

    // Singleton ImageLoader instance.
    ns.ImageLoader.instance = new ns.ImageLoader();

})(Core.ns('PlanBuilder'));