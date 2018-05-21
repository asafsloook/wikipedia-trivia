
importScripts('jquery.js'); 

self.onmessage = function (e) {

    var temp = articles[0];
    articles.shift();
    postMessage(temp);

};

articles = [];
goSearch();

function goSearch() {
    setInterval(function () {
        
        if (articles.length < 10) {
            var request = {
                userId: 193
            }
            getArticle(request);
        }

    }, 1000);
}

function getArticle(request) {

    var dataString = JSON.stringify(request);

    $.ajax({ // ajax call starts
        url: urlDomain + 'WebService.asmx/GetArticle',   // server side web service method
        data: dataString,                          // the parameters sent to the server
        type: 'POST',                              // can be also GET
        dataType: 'json',                          // expecting JSON datatype from the server
        contentType: 'application/json; charset = utf-8', // sent to the server
        success: successArticlesCB,                // data.d id the Variable data contains the data we get from serverside
        error: errorArticlesCB
    }); // end of ajax call

}


function successArticlesCB(results) {
    
    var results = $.parseJSON(results.d);
    articles.push(results);
    postMessage(results);
    
}

function errorArticlesCB(e) {

    console.log('Error in getArticle: ' + e);

}