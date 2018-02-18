
function getArticle(successArticlesCB, errorArticlesCB) {
    $.ajax({ // ajax call starts
        url: './WebService.asmx/GetArticle',   // server side web service method
        //data: dataString,                          // the parameters sent to the server
        type: 'POST',                              // can be also GET
        dataType: 'json',                          // expecting JSON datatype from the server
       // contentType: 'application/json; charset = utf-8', // sent to the server
        success: successArticlesCB,                // data.d id the Variable data contains the data we get from serverside
        error: errorArticlesCB
    }); // end of ajax call
}