﻿var GOOGLE_FONT_URL = 'https://fonts.gstatic.com';
var CACHE_STATIC_NAME = 'pwanote-static_v1';
var CACHE_DYNAMIC_NAME = 'pwanote-dynamic_v1';
var STATIC_ASSETS = [
    '/',
    '~/PWA/manifest.json',
];


self.addEventListener('install', function (event) {

    event.waitUntil(
        caches.open(CACHE_STATIC_NAME)
            .then(function (cache) {
                console.log('[SW] Precaching App Shell');
                return cache.addAll(STATIC_ASSETS);
            })
            .catch(function (e) {
                console.log('[SW] Precaching Error !', e);
            })
    );

});

self.addEventListener('activate', function (event) {
    event.waitUntil(
        caches.keys().then(function (keyList) {
            return Promise.all(
                keyList.map(function (key) {
                    if (key !== CACHE_STATIC_NAME && key !== CACHE_DYNAMIC_NAME) {
                        return caches.delete(key);
                    }
                }));
        }));
    return self.clients.claim();
});


function isIncluded(string, array) {
    var path;
    if (string.indexOf(self.origin) === 0) {
        path = string.substring(self.origin.length);
    } else {
        // for CDNs
        path = string;
    }
    //return array.indexOf(path) > -1;
    return array.includes(path) > -1;
}

var isGoogleFont = function (request) {
    return request.url.indexOf(GOOGLE_FONT_URL) === 0;
}

var cacheGFonts = function (request) {
    return fetch(request)
        .then(function (newRes) {
            caches.open(CACHE_DYNAMIC_NAME)
                .then(function (cache) {
                    cache.put(request, newRes);
                });
            return newRes.clone();
        });
};


self.addEventListener('fetch', function (event) {
    var request = event.request;
    // cacheOnly for statics assets
    if (isIncluded(request.url, STATIC_ASSETS)) {
        event.respondWith(caches.match(request));
    }
    // Runtime or Dynamic cache for google fonts
    if (isGoogleFont(request)) {
        event.respondWith(
            caches.match(request)
                .then(function (res) {
                    return res || cacheGFonts(request);
                })
        );
    }
});