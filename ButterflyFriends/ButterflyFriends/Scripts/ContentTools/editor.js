window.addEventListener('load', function () {
    var editor;

    ContentTools.StylePalette.add([new ContentTools.Style('By-line', 'article__by-line', ['p']),
             new ContentTools.Style('Caption', 'article__caption', ['p']),
             new ContentTools.Style('Example', 'example', ['pre']),
             new ContentTools.Style('Example + Good', 'example--good', ['pre']),
             new ContentTools.Style('Example + Bad', 'example--bad', ['pre']),
             new ContentTools.Style('Author', 'author', ['p'])]);

    editor = ContentTools.EditorApp.get();
    editor.init('[data-editable], [data-fixture]', 'data-name');

    editor.addEventListener('saved', function (ev) {
        var name, payload, regions, xhr;
        // Check that something changed
        regions = ev.detail().regions;
        console.log(regions);
        if (Object.keys(regions).length == 0) {
            return;
        }

        // Set the editor as busy while we save our changes
        this.busy(true);

        // Collect the contents of each region into a FormData instance
        payload = new FormData();
        for (name in regions) {
            if (regions.hasOwnProperty(name)) {
                payload.append(name, regions[name]);
            }
        }

        // Send the update content to the server to be saved
        /* function onStateChange(ev) {
             // Check if the request is finished
             if (ev.target.readyState == 4) {
                 editor.busy(false);
                 if (ev.target.status == '200') {
                     // Save was successful, notify the user with a flash
                     new ContentTools.FlashUI('ok');
                 } else {
                     // Save failed, notify the user with a flash
                     new ContentTools.FlashUI('no');
                 }
             }
         };*/

        $.ajax({
            url: '/Admin/PR/UploadArticle',
            async: true,
            contentType: false,//"application/json; charset=utf-8",
            processData: false,
            type: "POST",
            data: payload,
            xhr: function () {  // Custom XMLHttpRequest
                var myXhr = $.ajaxSettings.xhr();
                if (myXhr.upload) { // Check if upload property exists, method to be used for progress bars or the like

                    // For handling the progress of the upload
                    //myXhr.upload.id = id;
                    //myXhr.upload.addEventListener('progress', progressHandlingFunction, false);

                }
                return myXhr;
            }
        ,
            success: function (result) {
                if (result.error == true) {
                    $("#error").text(result.message);
                    new ContentTools.FlashUI('ok');
                }
                else if (result.success == true) {
                    $("#success").text(result.message);
                    new ContentTools.FlashUI('no');
                }

            },
            error: function (err) {
                console.log(err.statusText);
                new ContentTools.FlashUI('no');
            }
        });
        /*
        xhr = new XMLHttpRequest();
        xhr.addEventListener('readystatechange', onStateChange);
        xhr.open('POST', '/Admin/PR/UploadArticle');
        xhr.send(payload);*/
    });
    FIXTURE_TOOLS = [['undo', 'redo', 'remove']];
    ContentEdit.Root.get().bind('focus', function (element) {
        var tools;
        if (element.isFixed()) {
            tools = FIXTURE_TOOLS;
        } else {
            tools = ContentTools.DEFAULT_TOOLS;
        }
        if (editor.toolbox().tools() !== tools) {
            return editor.toolbox().tools(tools);
        }
    });

});

/*
(function () {
    var ImageUploader;

    ImageUploader = (function () {
        ImageUploader.imagePath = 'image.png';

        ImageUploader.imageSize = [600, 174];

        function ImageUploader(dialog) {
            this._dialog = dialog;
            this._dialog.addEventListener('cancel', (function (_this) {
                return function () {
                    return _this._onCancel();
                };
            })(this));
            this._dialog.addEventListener('imageuploader.cancelupload', (function (_this) {
                return function () {
                    return _this._onCancelUpload();
                };
            })(this));
            this._dialog.addEventListener('imageuploader.clear', (function (_this) {
                return function () {
                    return _this._onClear();
                };
            })(this));
            this._dialog.addEventListener('imageuploader.fileready', (function (_this) {
                return function (ev) {
                    return _this._onFileReady(ev.detail().file);
                };
            })(this));
            this._dialog.addEventListener('imageuploader.mount', (function (_this) {
                return function () {
                    return _this._onMount();
                };
            })(this));
            this._dialog.addEventListener('imageuploader.rotateccw', (function (_this) {
                return function () {
                    return _this._onRotateCCW();
                };
            })(this));
            this._dialog.addEventListener('imageuploader.rotatecw', (function (_this) {
                return function () {
                    return _this._onRotateCW();
                };
            })(this));
            this._dialog.addEventListener('imageuploader.save', (function (_this) {
                return function () {
                    return _this._onSave();
                };
            })(this));
            this._dialog.addEventListener('imageuploader.unmount', (function (_this) {
                return function () {
                    return _this._onUnmount();
                };
            })(this));
        }

        ImageUploader.prototype._onCancel = function () { };

        ImageUploader.prototype._onCancelUpload = function () {
            clearTimeout(this._uploadingTimeout);
            return this._dialog.state('empty');
        };

        ImageUploader.prototype._onClear = function () {
            return this._dialog.clear();
        };

        ImageUploader.prototype._onFileReady = function (file) {
            var upload;
            console.log(file);
            this._dialog.progress(0);
            this._dialog.state('uploading');
            upload = (function (_this) {
                return function () {
                    var progress;
                    progress = _this._dialog.progress();
                    progress += 1;
                    if (progress <= 100) {
                        _this._dialog.progress(progress);
                        return _this._uploadingTimeout = setTimeout(upload, 25);
                    } else {
                        return _this._dialog.populate(ImageUploader.imagePath, ImageUploader.imageSize);
                    }
                };
            })(this);
            return this._uploadingTimeout = setTimeout(upload, 25);
        };

        ImageUploader.prototype._onMount = function () { };

        ImageUploader.prototype._onRotateCCW = function () {
            var clearBusy;
            this._dialog.busy(true);
            clearBusy = (function (_this) {
                return function () {
                    return _this._dialog.busy(false);
                };
            })(this);
            return setTimeout(clearBusy, 1500);
        };

        ImageUploader.prototype._onRotateCW = function () {
            var clearBusy;
            this._dialog.busy(true);
            clearBusy = (function (_this) {
                return function () {
                    return _this._dialog.busy(false);
                };
            })(this);
            return setTimeout(clearBusy, 1500);
        };

        ImageUploader.prototype._onSave = function () {
            var clearBusy;
            this._dialog.busy(true);
            clearBusy = (function (_this) {
                return function () {
                    _this._dialog.busy(false);
                    return _this._dialog.save(ImageUploader.imagePath, ImageUploader.imageSize, {
                        alt: 'Example of bad variable names'
                    });
                };
            })(this);
            return setTimeout(clearBusy, 1500);
        };

        ImageUploader.prototype._onUnmount = function () { };

        ImageUploader.createImageUploader = function (dialog) {
            return new ImageUploader(dialog);
        };

        return ImageUploader;

    })();

    window.ImageUploader = ImageUploader;

    window.onload = function () {
        var FIXTURE_TOOLS, editor, req;
        ContentTools.IMAGE_UPLOADER = ImageUploader.createImageUploader;
        ContentTools.StylePalette.add([new ContentTools.Style('By-line', 'article__by-line', ['p']),
            new ContentTools.Style('Caption', 'article__caption', ['p']),
            new ContentTools.Style('Example', 'example', ['pre']),
            new ContentTools.Style('Example + Good', 'example--good', ['pre']),
            new ContentTools.Style('Example + Bad', 'example--bad', ['pre']),
            new ContentTools.Style('Author', 'author', ['p'])]);
        editor = ContentTools.EditorApp.get();
        editor.init('[data-editable], [data-fixture]', 'data-name');
        editor.addEventListener('saved', function (ev) {
            var name, payload, regions, xhr;
            console.log(ev.detail().regions);
            // Check that something changed
            regions = ev.detail().regions;
            if (Object.keys(regions).length == 0) {
                return;
            }

            // Set the editor as busy while we save our changes
            this.busy(true);

            // Collect the contents of each region into a FormData instance
            payload = new FormData();
            for (name in regions) {
                if (regions.hasOwnProperty(name)) {
                    payload.append(name, regions[name]);
                }
            }

            // Send the update content to the server to be saved
            function onStateChange(ev) {
                // Check if the request is finished
                if (ev.target.readyState == 4) {
                    editor.busy(false);
                    if (ev.target.status == '200') {
                        // Save was successful, notify the user with a flash
                        new ContentTools.FlashUI('ok');
                    } else {
                        // Save failed, notify the user with a flash
                        new ContentTools.FlashUI('no');
                    }
                }
            };

            $.ajax({
                url: '/Admin/PR/UploadArticle',
                async: true,
                contentType: false,//"application/json; charset=utf-8",
                processData: false,
                type: "POST",
                data: payload,
                xhr: function () {  // Custom XMLHttpRequest
                    var myXhr = $.ajaxSettings.xhr();
                    if (myXhr.upload) { // Check if upload property exists, method to be used for progress bars or the like

                        // For handling the progress of the upload
                        //myXhr.upload.id = id;
                        //myXhr.upload.addEventListener('progress', progressHandlingFunction, false);

                    }
                    return myXhr;
                }
            ,
                success: function (result) {
                    if (result.error == true) {
                        $("#error").text(result.message);
                        new ContentTools.FlashUI('ok');
                    }
                    else if (result.success == true) {
                        $("#success").text(result.message);
                        new ContentTools.FlashUI('no');
                    }

                },
                error: function (err) {
                    console.log(err.statusText);
                    new ContentTools.FlashUI('no');
                }
            });
            
            xhr = new XMLHttpRequest();
            xhr.addEventListener('readystatechange', onStateChange);
            xhr.open('POST', '/Admin/PR/UploadArticle');
            xhr.send(payload);
        });
        FIXTURE_TOOLS = [['undo', 'redo', 'remove']];
        ContentEdit.Root.get().bind('focus', function (element) {
            var tools;
            if (element.isFixed()) {
                tools = FIXTURE_TOOLS;
            } else {
                tools = ContentTools.DEFAULT_TOOLS;
            }
            if (editor.toolbox().tools() !== tools) {
                return editor.toolbox().tools(tools);
            }
        });
        req = new XMLHttpRequest();
        req.overrideMimeType('application/json');
        req.open('GET', 'https://raw.githubusercontent.com/GetmeUK/ContentTools/master/translations/lp.json', true);
        return req.onreadystatechange = function (ev) {
            var translations;
            if (ev.target.readyState === 4) {
                translations = JSON.parse(ev.target.responseText);
                ContentEdit.addTranslations('lp', translations);
                return ContentEdit.LANGUAGE = 'lp';
            }
        };
    };

}).call(this);
*/
    