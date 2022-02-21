
$(document).ready(function () {

    if (window.location.href.toString().indexOf('index.html') != -1) {


        $('#splashLogo').fadeOut(700).fadeIn(700);
        splashHandle = setInterval(function () {
            $('#splashLogo').fadeOut(700).fadeIn(700);
        }, 1400);

    }

    if (window.location.href.toString().indexOf('article.html') != -1) {


        showLoading();
    }


});

$(document).ready(function () {
    $('.premium').on('click', function () {
        $('.material-menu').removeClass('opacity-out');
        $('.sidebar-left, .sidebar-right').removeClass('active-sidebar-box');
        $('.sidebar-tap-close').removeClass('active-tap-close');
        $("#page-content, .header, .footer-menu").css({
            "transform": "translateX(0px)",
            "-webkit-transform": "translateX(0px)",
            "-moz-transform": "translateX(0px)",
            "-o-transform": "translateX(0px)",
            "-ms-transform": "translateX(0px)"
        });
    });
});

function goSearch() {

    searcher();

    searchThread = setInterval(searcher, 2500);
}

function searcher() {
    if (localStorage.articles != null) {
        articles = $.parseJSON(localStorage.articles);
    }

    var uid = parseInt(localStorage.Id);

    if (articles.length < 25) {

        if (window.location.href.toString().indexOf('profile.html') != -1) {
            if (articles.length < 2) {
                $('#loading').show();
                $('.lodingGameText').show();
                $('.profileMenu').hide();
            }
            else {
                $('.lodingGameText').fadeOut();
                $('#loading').fadeOut();
                $('.profileMenu').fadeIn();
            };
        }

        if (articles.length < 2) {
            $(".sidebar-menu a[href*='article.html']").hide();
            $(".sidebar-menu #loading2").show();

            $(".profileMenu a[href*='article.html']").hide();
        }
        else {
            $(".sidebar-menu a[href*='article.html']").show();
            $(".sidebar-menu #loading2").hide();

            $(".profileMenu a[href*='article.html']").show();
        };

        
        var request = {
            userId: uid
        }
        getArticle(request);
    }
    else {
        return;
    }
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


function successArticlesCB(data) {

    var newArticle = $.parseJSON(data.d);

    if (newArticle.ArticleId == null) {

        //var badCategory = newArticle.ArticleContent;

        //alert("Please remove this category from your preferences: " + badCategory);
        return;
    }

    if (localStorage.articles != null) {
        articles = $.parseJSON(localStorage.articles);
    }

    for (var i = 0; i < articles.length; i++) {
        if (articles[i].ArticleId == newArticle.ArticleId) {
            return;
        }
    }

    articles.push(newArticle);

    localStorage.articles = JSON.stringify(articles);
}

function errorArticlesCB(e) {

    console.log('Error in getArticle: ' + e.responseText);

}


function articleFromLS() {

    if (typeof searchThread === 'undefined') {
        goSearch();
    }

    articles = $.parseJSON(localStorage.articles);

    if (typeof articles !== 'undefined' && articles.length > 0) {
        //showArticle
        showArticleOrQuest();
    }
    else {
        //wait

        setTimeout(articleFromLS, 1000);
    }

}

function showArticleOrQuest() {
    //setTimeout(function () {

    var temp = [];
    temp = $.parseJSON(localStorage.articles)
    var article = temp[0];

    var oldArticles = $.parseJSON(localStorage.articles);
    oldArticles.shift();
    localStorage.articles = JSON.stringify(oldArticles);

    try {
        //check if notification is question
        if (typeof article.NotificationContent !== 'undefined' && article.NotificationContent.indexOf('?') == article.NotificationContent.length - 1) {
            Question = article.NotificationContent;
            Question = Question.replace(" ,", ",");
            Question = Question.replace(" ;", ";");
            articleIdForScore = article.ArticleId;
            findAns(article.Title);
        }
        else {
            articleFromLS();
            return;
        }
    } catch (e) {
        articleFromLS();
        return;
    }


    $("#shareBTN").show();

    if (article.PhotoUrl == null) {
        $("#ArticleImg").hide();
    }
    else {
        $("#ArticleImg").attr("src", article.PhotoUrl);
        $("#ArticleImg").show();
    }


    $("#title").empty();
    $("#title").html(article.Title);

    //results.ArticleContent
    $("#articleContent").empty();
    //$("#articleContent").append("<b>Root category:</b> " + results.Category.Name + "<br><br>");


    var content = article.ArticleContent;

    content = content.replace(/<p\s+class="mw-empty-elt">[\S\s]*?<\/p>/gi, '');
    content = content.replace(/<span><\/span>/gi, '');
    content = content.replace(/<p>[\n\r]+<\/p>/gi, '');
    content = content.replace(/<p><\/p>/gi, '');
    $("#articleContent").append(content);


    //ajax to server, user read this article
    var uid = parseInt(localStorage.Id);
    var request = {
        userId: uid,
        rootCategory: article.Category.Name,
        articleId: article.ArticleId

    }
    readArticle(request);

    //}, 2000);
}

function readArticle(request) {

    var dataString = JSON.stringify(request);

    $.ajax({ // ajax call starts
        url: urlDomain + 'WebService.asmx/readArticle',   // server side web service method
        data: dataString,                          // the parameters sent to the server
        type: 'POST',                              // can be also GET
        dataType: 'json',                          // expecting JSON datatype from the server
        contentType: 'application/json; charset = utf-8', // sent to the server
        success: successReadArticleCB,                // data.d id the Variable data contains the data we get from serverside
        error: errorReadArticleCB
    }); // end of ajax call

}

function successReadArticleCB(data) {
    var x = data;
}

function errorReadArticleCB(e) {
    //alert("error in readArticle: " + e.responseText);
}

function showLoading() {

    $("#loading").show();
    $('#page-content-scroll').hide();
    $('.back-to-top-badge').hide();
    $("#refreshBTN").hide();
    $("#burgerMenu").hide();
}

function hideLoading() {
    setTimeout(function () {
        $("#loading").fadeOut();
        $("#refreshBTN").fadeIn();
        $('.back-to-top-badge').show();
        $('#page-content-scroll').fadeIn();
        $('#page-content-scroll').scrollTop(0);
        $("#burgerMenu").show();

        //stop typer thread
        typer = false;
    }, 500)
}


function showArticle() {
    $("#refreshBTN").fadeIn();
    $('.back-to-top-badge').show();
    $('#page-content-scroll').fadeIn();
    $('#page-content-scroll').scrollTop(0);
    $("#burgerMenu").show();
}


function hideLoadingQuest() {

    t2 = new Date();
    var dif = t1.getTime() - t2.getTime();
    var Seconds_from_T1_to_T2 = dif / 1000;
    var Seconds_Between_Dates = Math.abs(Seconds_from_T1_to_T2);
    console.log(Seconds_Between_Dates + " seconds");

    $('#questionDiv').show();
    $("#loading").fadeOut();

    //stop typer thread
    typer = false;
}






function closeAllInfoWindows() {

    if (typeof InfoWindows !== 'undefined') {
        for (var i = 0; i < InfoWindows.length; i++) {
            InfoWindows[i].close();
        }
    }
}

function onSuccess(position) {


    //location based
    localStorage.lastLAT = position.coords.latitude;
    localStorage.lastLNG = position.coords.longitude;
    map.setCenter(new google.maps.LatLng(position.coords.latitude, position.coords.longitude));

    //haifa
    //localStorage.lastLAT = 32.814103;
    //localStorage.lastLNG = 34.996188;
    //map.setCenter(new google.maps.LatLng(32.814103, 34.996188));

    //nyc
    //localStorage.lastLAT =  40.73152200000;
    //localStorage.lastLNG = -73.99736000000;
    //map.setCenter(new google.maps.LatLng(40.731522, -73.997360));

    wiki();

}

function onError(error) {
    alert("Please activate Location services and refresh the page");
    getNewWiki();
}

function getMyPosition() {

    var options = {
        enableHighAccuracy: true,
        timeout: 5000,
        maximumAge: 10000
    };
    navigator.geolocation.getCurrentPosition(onSuccess, onError, options);
}

function clearAll() {
    if (typeof markers !== 'undefined') {
        while (markers.length) { markers.pop().setMap(null); }
        markers.length = 0;
    }
}

function wiki() {

    clearAll();

    //wiki geo call
    $.ajax({

        url: 'https://en.wikipedia.org/w/api.php?action=query&list=geosearch&gscoord='
        + localStorage.lastLAT.toString().substring(0, 10) + '%7C'
        + localStorage.lastLNG.toString().substring(0, 10)
        + '&gsradius=5000&gslimit=100&format=json',

        dataType: "jsonp",
        success: function (data) {
            var x = data;
            markers = [];
            InfoWindows = [];

            for (var i = 0; i < data.query.geosearch.length; i++) {

                var lat = data.query.geosearch[i].lat;
                var lng = data.query.geosearch[i].lon;
                var dist = data.query.geosearch[i].dist;
                var pageid = data.query.geosearch[i].pageid;
                var title_ = data.query.geosearch[i].title;

                var latLng = new google.maps.LatLng(lat, lng);
                var marker = new google.maps.Marker({
                    position: latLng,
                    map: map,
                    title: title_,
                    id: pageid,
                    optimized: false,
                    clickable: true
                });

                markers.push(marker);
            }

            if (markers.length > 75) {
                map.setZoom(15);
            }
            else if (markers.length > 60) {
                map.setZoom(14);
            }
            else if (markers.length > 50) {
                map.setZoom(13);
            }
            else if (markers.length > 20) {
                map.setZoom(12);
            }
            else {
                map.setZoom(11);
            }

            for (var i = 0; i < markers.length; i++) {

                var title_ = data.query.geosearch[i].title;

                //add space for long titles
                if (title_.length > 19) {
                    var temp = title_.split(" ");
                    var mid = parseInt(temp.length / 2) + 1;
                    var space = "<br>";
                    temp.splice(mid, 0, space);
                    title_ = temp.join(" ");
                }

                var contentString = '<div><h5 style="text-align:center"><a href="articlearound.html">' + title_ + '</a></h5><img class="infoWindowPic" id="' + markers[i].id + 'pic" /></div>';

                markers[i].infowindow = new google.maps.InfoWindow({
                    content: contentString
                });

                InfoWindows.push(markers[i].infowindow);

                google.maps.event.addListener(markers[i], 'click', function () {

                    getArticlePhoto(this.title);

                    localStorage.lastPageid = this.id;
                    localStorage.lastPageTitle = this.title;

                    closeAllInfoWindows();

                    this.infowindow.open(map, this);
                });

            }
            if (typeof fromSearchBox !== 'undefined' && fromSearchBox) {
                fromSearchBox = null;
                new google.maps.event.trigger(markers[0], 'click');

            }
        },
        error: function (data) {
            var y = data;
        }
    });

}


function getNewWiki() {

    var c = map.getCenter();

    localStorage.lastLAT = c.lat();
    localStorage.lastLNG = c.lng();

    wiki();
}


function getArticleById(pageid) {
    $.ajax({

        url: 'https://en.wikipedia.org/w/api.php?action=query&prop=extracts&format=json&pageids=' + pageid,
        dataType: "jsonp",
        success: function (data) {
            var x = data.query.pages[Object.keys(data.query.pages)[0]].extract;

            filtersIDS = ['References', 'Gallery', 'See_also', 'Sources', 'Notes', 'External_links'];

            for (var i = 0; i < filtersIDS.length; i++) {

                if (x.indexOf('<h2><span id="' + filtersIDS[i] + '">') != -1) {
                    x = x.substring(0, x.indexOf('<h2><span id="' + filtersIDS[i] + '">'));
                }
            }
            $("#aroundContent").html(x);

            getArticlePhoto(localStorage.lastPageTitle);

        }
    });
}


function getArticlePhoto(title) {
    $.ajax({

        url: 'https://en.wikipedia.org/w/api.php?action=query&format=json&generator=prefixsearch&gpssearch=' + title + '&gpslimit=1&prop=pageimages%7Cpageterms&piprop=thumbnail&pithumbsize=250&pilimit=10&redirects=&wbptterms=description',
        dataType: "jsonp",
        success: function (data) {

            try {
                var x = data.query.pages[Object.keys(data.query.pages)[0]].thumbnail.source;
            }
            catch (err) {

            }


            if (window.location.href.toString().indexOf('articlearound.html') != -1) {

                $("#aroundPhoto").hide();

                if (x != null) {
                    $("#aroundPhoto").attr("src", x);
                    $("#aroundPhoto").show();
                }

                $("#aroundTitle").html(localStorage.lastPageTitle);

            }
            else if (window.location.href.toString().indexOf('aroundme.html') != -1) {
                var selector = "#" + localStorage.lastPageid + "pic";
                $(selector).attr("src", x);
            }

        }
    });
}


function loadingTyper() {

    $("#animationTitle").html("");


    txts = [' interesting', ' amazing', ' fascinating', ' impressive', ' delightful', ' striking', ' pleasing', ' lovely', ' refreshing', ' intriguing'];
    var x = Math.floor(Math.random() * txts.length);
    txtCounter = x;

    txt = txts[txtCounter];

    iTW = 0;

    typer = true;

    typeWriter();

}

function typeWriter() {

    if (window.location.href.toString().indexOf('article.html') == -1 || !typer) {
        return;
    }

    //writing chars of word
    if (iTW < txt.length) {
        $("#animationTitle").append(txt.charAt(iTW));
        iTW++;
        setTimeout(typeWriter, 120);
        return;
    }
    //end of writing word
    if (iTW == txt.length) {
        setTimeout(typeDelete, 250);
        return;
    }
    return;
}

function typeDelete() {

    if (window.location.href.toString().indexOf('article.html') == -1 || !typer) {
        return;
    }

    //delete word chars
    if (iTW != 0) {

        var title = $("#animationTitle").html();
        $("#animationTitle").html(title.substring(0, iTW - 1));
        iTW--;
        setTimeout(typeDelete, 75);
        return;
    }
    //end of delete word
    if (iTW == 0) {
        if (txtCounter == txts.length - 1) {
            txtCounter = -1;
        }
        txt = txts[++txtCounter];
        setTimeout(typeWriter, 120);
        return;
    }
    return;
}

function findAns(title) {

    //get claims, like P31 -> Q214070

    $.ajax({

        url: 'https://www.wikidata.org/w/api.php?format=json&action=wbgetentities&sites=enwiki&titles=' + title,
        dataType: "jsonp",
        success: function (data) {
            var wikidataID = "";
            try {
                wikidataID = data.entities[Object.keys(data.entities)[0]].id;
                var allClaims = data.entities[Object.keys(data.entities)[0]].claims;

                //P279
                var x = allClaims.P279[0].mainsnak.datavalue.value.id;

                var query = [];
                query.P = "P279";
                query.Q = x;

                findAnsCon(query, title, wikidataID);
            }
            catch (e) {
                try {
                    //P31 - instance of
                    var x = allClaims.P31[0].mainsnak.datavalue.value.id;

                    //if Q5 (human) try P39(position held) else go to P106(occupation)
                    if (x == "Q5") {
                        try {

                            //P39
                            var x = allClaims.P39[0].mainsnak.datavalue.value.id;

                            var query = [];
                            query.P = "P39";
                            query.Q = x;

                            findAnsCon(query, title, wikidataID);

                        } catch (e) {
                            try {

                                //P106
                                var x = allClaims.P106[0].mainsnak.datavalue.value.id;

                                var query = [];
                                query.P = "P106";
                                query.Q = x;

                                findAnsCon(query, title, wikidataID);
                            } catch (e) {
                                try {
                                    //P27
                                    var x = allClaims.P27[0].mainsnak.datavalue.value.id;

                                    var query = [];
                                    query.P = "P27";
                                    query.Q = x;

                                    findAnsCon(query, title, wikidataID);

                                } catch (e) {
                                    articleFromLS();
                                    return;
                                }
                            }
                        }
                    }
                    else {

                        try {
                            //P360 - list
                            var x = allClaims.P360[0].mainsnak.datavalue.value.id;

                            articleFromLS();
                            return;

                        } catch (e) {

                        }

                        var query = [];
                        query.P = "P31";
                        query.Q = x;

                        findAnsCon(query, title, wikidataID);
                    }

                } catch (e) {
                    articleFromLS();
                    return;
                }
            }
        }
    });

}

function findAnsCon(query, title, wikidataID) {
    //search for same P31 -> Q214070
    var endpointUrl = 'https://query.wikidata.org/sparql',

        sparqlQuery = "SELECT ?A WHERE {\n" +
            "  SERVICE wikibase:label { bd:serviceParam wikibase:language \"[AUTO_LANGUAGE],en\". }\n" +
            "  ?A wdt:" + query.P + " wd:" + query.Q + ".\n" +
            "}\n" +
            "LIMIT 49",
        settings = {
            headers: { Accept: 'application/sparql-results+json' },
            data: { query: sparqlQuery }
        };

    $.ajax(endpointUrl, settings).then(function (data) {
        answers = [];
        var results = data.results.bindings;

        answers.push(wikidataID);

        for (var i = 0; i < results.length; i++) {

            var item = results[i].A.value.replace("http://www.wikidata.org/entity/", "");

            if (answers.indexOf(item) == -1) {

                answers.push(item);
            }
        }

        translate(answers);
    });
}

function search(nameKey, myArray) {
    for (var i = 0; i < myArray.length; i++) {
        if (myArray[i].name === nameKey) {
            return myArray[i];
        }
    }
}

function translate(answers) {

    var url_ = "https://www.wikidata.org/w/api.php?format=json&action=wbgetentities&sites=enwiki&props=labels&ids=";

    for (var i = 0; i < answers.length; i++) {

        url_ += answers[i];

        if (i != answers.length - 1) {
            url_ += "|";
        }
    }

    $.ajax({

        url: url_,
        dataType: "jsonp",
        success: function (data) {

            var x = data.entities;

            stringAnswers = [];

            //add the right answer
            stringAnswers.push(x[answers[0]].labels.en.value);


            while (stringAnswers.length != 4 && stringAnswers.length != Object.keys(x).length) {

                var newAns = "";
                var rnd = 0;

                try {
                    rnd = Math.floor(Math.random() * Object.keys(x).length);
                    newAns = x[answers[rnd]].labels.en.value;

                } catch (e) {

                    for (var i = 0; i < x.length; i++) {

                    }

                    delete x[answers[rnd]];
                    delete answers[rnd];

                    answers.clean(undefined);

                    continue;
                }

                if (stringAnswers.indexOf(newAns) == -1) {
                    stringAnswers.push(newAns);
                }

                if (stringAnswers.length == 4) {
                    break;
                }
            }

            if (typeof Question !== 'undefined') {

                if (stringAnswers.length == 1) {


                    articleFromLS();
                }
                else {
                    showQuestion();
                }

            }
            else {
                //hideLoading();

                articleFromLS();
            }
        }
    });
}

Array.prototype.clean = function (deleteValue) {
    for (var i = 0; i < this.length; i++) {
        if (this[i] == deleteValue) {
            this.splice(i, 1);
            i--;
        }
    }
    return this;
};

function showQuestion() {
    $('#answers').empty();
    $('#question').empty();

    //stringAnswers //Question
    $('#question').append('<h4>' + Question + '<h4>');

    Question = undefined;

    correct = stringAnswers[0];

    //scrambble answers
    stringAnswers = shuffle(stringAnswers);

    for (var i = 0; i < stringAnswers.length; i++) {

        var ansToUpper = capitalizeFirstLetter(stringAnswers[i]);
        $('#answers').append('<label>' + ansToUpper + '</label>');
    }

    clicks = 0;

    $('#answers label').on('click', function () {

        clearInterval(timer);
        timer = undefined;
        $('#timer').hide();

        if (clicks > 0) {
            return;
        }

        var choose = $(this).html().toLowerCase();
        correct = correct.toLowerCase();
        score = 0;

        if (correct == choose) {
            $(this).css("background-color", "#4CAF50");
            //$(this).effect('shake', {
            //    direction: 'up',
            //    distance: 5,
            //    times: 3
            //});

            //update score for user
            var ans = stringAnswers.length;


            if (ans == 4) {
                score = 10;
            }
            else if (ans == 3) {
                score = 7;
            }
            else {
                //2
                score = 5;
            }

            var uid = parseInt(localStorage.Id);
            var request = {
                Score: score,
                UserId: uid,
                articleId: articleIdForScore
            }

            saveScore(request);

        }
        else {
            //wrong answer

            findRightAnswerElement();

            $(this).css("background-color", "#f44336");
            $(this).effect('shake', {
                distance: 5,
                times: 3
            });
        }


        setTimeout(function () {
            if (score == 0) {
                answerWrong();
            }
            else {
                answerRight();
            }
        }, 1000);


        setTimeout(function () {
            $('#questionDiv').fadeOut();
            showArticle();
        }, 3000);


        clicks++;
    });

    timer, n = 30;

    // events
    startTimer(n);
    $('#result').html(n);

    hideLoadingQuest();
}

function startTimer(n) {
    var i = n - 1;

    var canvas = document.getElementById('progress');
    var context = canvas.getContext('2d');
    context.clearRect(0, 0, canvas.width, canvas.height);
    result.style.color = '#00CE9B';

    timer = setInterval(function () {
        result.innerHTML = i--;

        stopTimer = function () {
            clearInterval(timer);
            result.innerHTML = i;
        }

        if (i < 5) {
            result.style.color = '#ED3E42';
        } // hurry up!

        if (i < 0) {
            setTimeout(function () {

                stopTimer();
                $('#timer').hide();
                $('#questionDiv').fadeOut();
                showArticle();
            }, 500);

        } // finish

        function updateProgress() {
            var canvas = document.getElementById('progress');
            var context = canvas.getContext('2d');
            var centerX = canvas.width / 2;
            var centerY = canvas.height / 2;
            var radius = 20;
            var circ = Math.PI * 2; // 360deg
            var percent = i / n; // i%
            context.beginPath();
            context.arc(centerX, centerY, radius, ((circ) * percent), circ, false);
            context.lineWidth = 5;
            if (i < 5) {
                context.lineWidth = 6;
                context.strokeStyle = '#ED3E42';
            } else {
                context.strokeStyle = '#00CE9B';
            }
            context.stroke();
        } // progress

        updateProgress();

    }, 1000); // every sec
    $('#timer').show();
}

function findRightAnswerElement() {

    var answers = $('#answers label');

    for (var i = 0; i < answers.length; i++) {

        var text = $('#answers label').eq(i)[0].innerHTML.toLowerCase();

        if (text == correct) {
            $('#answers label').eq(i).css("background-color", "#4CAF50");
        }
    }

}

function answerWrong() {

    var rnd = Math.floor((Math.random() * 10) + 1);
    var imgRND = 'w' + rnd;

    $("#dialog").html('<img class="dialogSmiley" src="images/' + imgRND + '.png"><h5 style="text-align:center">Don\'t worry, you\'ll get the next one</h5>');

    $("#dialog").dialog({
        title: "Wrong...",
        height: 100,
        position: { my: "center", at: "center", of: window },
        modal: true,
        hide: { effect: "fade", duration: 500 },
        show: { effect: "fade", duration: 250 },
        open: function (event, ui) {
            $(".ui-dialog-title").css('font-size', '20px');
            setTimeout("$('#dialog').dialog('destroy')", 3000);
            $(".ui-dialog-titlebar-close", ui.dialog | ui).hide();
        }
    });
}

function answerRight() {

    var rnd = Math.floor((Math.random() * 10) + 1);
    var imgRND = 'r' + rnd;

    $("#dialog").html('<img class="dialogSmiley" src="images/' + imgRND + '.png"><h5 style="text-align:center">you\'ve earned ' + score + ' points</h5>');

    $("#dialog").dialog({
        title: 'Correct!',
        height: 100,
        position: { my: "center", at: "center", of: window },
        modal: true,
        hide: { effect: "fade", duration: 500 },
        show: { effect: "fade", duration: 250 },
        open: function (event, ui) {
            $(".ui-dialog-title").css('font-size', '20px');
            setTimeout("$('#dialog').dialog('destroy')", 3000);
            $(".ui-dialog-titlebar-close", ui.dialog | ui).hide();
        }
    });
}

function saveScore(request) {
    var dataString = JSON.stringify(request);

    $.ajax({ // ajax call starts
        url: urlDomain + 'WebService.asmx/saveScore',   // server side web service method
        data: dataString,                          // the parameters sent to the server
        type: 'POST',                              // can be also GET
        dataType: 'json',                          // expecting JSON datatype from the server
        contentType: 'application/json; charset = utf-8', // sent to the server
        success: saveScoreSCB,                // data.d id the Variable data contains the data we get from serverside
        error: saveScoreECB
    }); // end of ajax call

}

function saveScoreSCB(data) {

}

function saveScoreECB(error) {
    //alert("Error in saveScore: " + error);
}

function shuffle(array) {
    var currentIndex = array.length, temporaryValue, randomIndex;

    // While there remain elements to shuffle...
    while (0 !== currentIndex) {

        // Pick a remaining element...
        randomIndex = Math.floor(Math.random() * currentIndex);
        currentIndex -= 1;

        // And swap it with the current element.
        temporaryValue = array[currentIndex];
        array[currentIndex] = array[randomIndex];
        array[randomIndex] = temporaryValue;
    }

    return array;
}

function capitalizeFirstLetter(string) {
    try {
        return string.charAt(0).toUpperCase() + string.slice(1);
    } catch (e) {
        return string;
    }
}

function uuidv4() {
    return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    );
}



$(document).ready(function () {

    function init_template() {//Class is vital to run AJAX Pages 


        $(function () {


            $('.button, .icon, .footer-menu-item i, .footer-menu-controls a, .mobileui-home a i, .landing-homepage ul li a i').on('click', function (event) {
                //event.preventDefault();

                var $div = $('<div/>'),
                    btnOffset = $(this).offset(),
                    xPos = event.pageX - btnOffset.left,
                    yPos = event.pageY - btnOffset.top;
                $div.addClass('button-effect');
                var $ripple = $(".button-effect");

                $ripple.css("height", $(this).height());
                $ripple.css("width", $(this).height());
                $div
                    .css({
                        top: yPos - ($ripple.height() / 2),
                        left: xPos - ($ripple.width() / 2),
                        background: "rgba(255,255,255,0.5)" //$(this).data("ripple-color")
                    })
                    .appendTo($(this));

                window.setTimeout(function () {
                    $div.remove();
                }, 800);
            });

        });

        //Timeout required to allow menu scrolling to bottom position
        setTimeout(function () {
            $('.footer-menu .footer-menu-item').addClass('remove-menu');
            $('.footer-menu-wrapper').addClass('remove-wrapper');
            $('.footer-menu-wrapper').removeClass('remove-menu');
            $('.page-menu').addClass('remove-menu');
        }, 0.01);


        urlDomain = "";
        if (window.location.href.toString().indexOf('http') == -1) {

            urlDomain = 'https://proj.ruppin.ac.il/bgroup54/test2/tar1/';

            document.addEventListener("deviceready", onDeviceReady, false);
        }
        else {
            urlDomain = '../';
            userPref = [];

            if (!localStorage.uuid) {
                localStorage.uuid = uuidv4();
                if (window.location.href.toString().indexOf('index.html') === -1) window.location.href = "https://grasp.azurewebsites.net/index.html";
            }
            if (window.location.href === 'https://grasp.azurewebsites.net/')
                window.location.href = "https://grasp.azurewebsites.net/index.html";
                

            //localStorage.uuid = "test";
            //userPref.Id = 197;
            //localStorage.Id = 197;

            //localStorage.uuid = "67a8bacd24573639";
            //userPref.Id = 192;
            //localStorage.Id = 192;

            onDeviceReady();
        }


        function onDeviceReady() {

            if (window.location.href.toString().indexOf('index.html') != -1) {

                if (localStorage.uuid == null || localStorage.uuid == undefined) {
                    localStorage.uuid = device.uuid;
                }

                if (typeof PushNotification !== 'undefined') {

                    var push = PushNotification.init({
                        android: {
                            senderID: "198839105363",
                            forceShow: true // this identifies your application
                            // it must be identical to what appears in the
                            // config.xml
                        },
                        browser: {
                            //pushServiceURL: 'http://push.api.phonegap.com/v1/push'
                        },
                        ios: {
                            alert: "true",
                            badge: "true",
                            sound: "true"
                        },
                        windows: {}
                    });

                    //-----------------------------------------------------------------------
                    // triggred by the notification server once the registration is completed
                    //-----------------------------------------------------------------------
                    push.on('registration', function (data) {

                        localStorage.RegId = data.registrationId;

                    });

                    //-------------------------------------------------------------
                    // triggred by a notification sent from the notification server
                    //-------------------------------------------------------------
                    push.on('notification', function (data) {

                        if (data.additionalData.foreground == true) {
                            //
                        }
                        else if (data.additionalData.coldstart == true) {
                            //
                        }
                        else {
                            //
                        }

                        if (data.additionalData.info == 'PhotoPush') {

                            localStorage.toPhotoPush = 'true';
                            window.location.replace('allphotos.html');
                        }
                        else {

                        }
                    });

                    //-----------------------------------------------------------
                    // triggred when there is an error in the notification server
                    //-----------------------------------------------------------
                    push.on('error', function (e) {
                        //alert(e.responseText);
                    });
                }

                var request = {
                    IMEI: localStorage.uuid
                }
                checkUser2(request);

                navigator.geolocation.getCurrentPosition();
            }

        }


        if (window.location.href.toString().indexOf('allphotos.html') != -1) {

            getPhotos();

            function checkPhotoPush() {
                if (localStorage.toPhotoPush == 'true') {
                    localStorage.toPhotoPush = 'false';
                    $('#ph9').click();
                }
            }

            $("#photos a[id*='ph']").on('click', function () {

                var id = parseInt(this.id.toString().substring(2, 3));

                localStorage["PhotoUrl"] = photos[id].Url;
                localStorage["PhotoDesc"] = photos[id].Description;
                localStorage["PhotoDate"] = photos[id].Date;
            });

            function getPhotos() {

                $.ajax({ // ajax call starts
                    url: urlDomain + 'WebService.asmx/GetPhotos',   // server side web service method
                    //data: dataString,                          // the parameters sent to the server
                    type: 'POST',                              // can be also GET
                    dataType: 'json',                          // expecting JSON datatype from the server
                    contentType: 'application/json; charset = utf-8', // sent to the server
                    success: successPhotosCB,                // data.d id the Variable data contains the data we get from serverside
                    error: errorPhotosCB
                }); // end of ajax call

            }

            function successPhotosCB(results) {
                var results = $.parseJSON(results.d);
                photos = results;

                for (var i = 0; i < results.length; i++) {

                    $("#ph" + i + " img").attr("src", results[i].Url);

                    $("#ph" + i + " strong").html((new Date(parseInt(results[i].Date.replace('/Date(', ''))).toLocaleDateString()));

                    var short = "";
                    if (results[i].Description.length > 60) {
                        short = "...";
                    }

                    $("#ph" + i + " em").html(results[i].Description.substring(0, 40).trim() + short);

                    checkPhotoPush();
                }

            }

            function errorPhotosCB(e) {
                //alert("I caught the exception : failed in GetPhotos \n The exception message is : " + e.responseText);
            }


        }

        if (window.location.href.toString().indexOf("article.html") != -1) {

            $('#timer').hide();


            showLoading();
            loadingTyper();

            articleFromLS();
            t1 = new Date();

            $('.footer-menu-open').click(function () {

                showLoading();
                loadingTyper();

                articleFromLS();
                t1 = new Date();

            });

        }

        if (window.location.href.toString().indexOf('photo.html') != -1) {

            renderPhoto();

            function renderPhoto() {

                var a1 = localStorage["PhotoUrl"];
                var a2 = localStorage["PhotoDate"];
                var a3 = localStorage["PhotoDesc"];


                $("#Photo").attr("src", a1);
                $("#title").html("Photo of the Day: " + (new Date(parseInt(a2.replace('/Date(', ''))).toLocaleDateString()));
                $("#articleContent").html(a3);

            }
        }

        if (window.location.href.toString().indexOf('pref1.html') != -1) {

            $("#burgerMenu").hide();

            cat = [];

            var request = {
                IMEI: localStorage.uuid
            }

            checkUser(request);


            function checkUser(request) {

                var dataString = JSON.stringify(request);

                $.ajax({ // ajax call starts
                    url: urlDomain + 'WebService.asmx/checkUser',   // server side web service method
                    data: dataString,                          // the parameters sent to the server
                    type: 'POST',                              // can be also GET
                    dataType: 'json',                          // expecting JSON datatype from the server
                    contentType: 'application/json; charset = utf-8', // sent to the server
                    success: checkUserSCB,                // data.d id the Variable data contains the data we get from serverside
                    error: checkUserECB
                }); // end of ajax call

            }

            function checkUserSCB(results) {

                var results = $.parseJSON(results.d);

                userPref = results;

                if (userPref.Categories == null || userPref.Categories.length == 0) {
                    $('#forwardBTN1').hide;
                }

                printUserCategories();
                checkPrefList();
            }

            function checkUserECB(e) {
                //alert("I caught the exception : failed in checkUser \n The exception message is : " + e.responseText);

            }


            function printUserCategories() {
                var str = "";

                if (userPref.Categories == null || userPref.Categories.length == 0) {
                    return;
                }

                userPref.Categories.forEach(function (element) {
                    str += '<div id="' + element.Name.replace(/\W/g, '') + '">' + element.Name
                        + '<a href="#" class="itemDelete"><i class="material-icons">delete</i></a>'
                        + '</div>';
                });

                $('.page-interests')[0].innerHTML += str;

                checkPrefList();
            }


            $("#tags").autocomplete({
                source: function (request, response) {
                    $.ajax({
                        url: 'https://en.wikipedia.org/w/api.php?action=opensearch&search=' + $("#tags").val() + '&limit=10&namespace=0&format=json',
                        dataType: "jsonp",
                        global: false,
                        success: function (data) {
                            response(data[1]);
                        }
                    });
                },
                minLength: 1
            });


            function checkIfExist(id) {
                var test = null;
                try {
                    var selector = '#' + id;
                    test = $(selector).html();

                } catch (e) {

                }
                if (test == null || test == undefined) {
                    return false;
                }
                return true;
            }

            $('#addBTN').on('click', function () {

                if ($("#tags").val().trim() == "") {
                    alert("Please choose a category");
                    return;
                }

                var choose = $("#tags").val().replace(/\W/g, '');

                if (checkIfExist(choose)) {
                    alert("exist");
                    return;
                }

                var url = 'https://en.wikipedia.org/wiki/Special:RandomInCategory/' + choose;
                //check if category has data

                var str = '<div id="' + choose + '">' + $("#tags").val()
                    + '<a href="#" class="itemDelete"><i class="material-icons">delete</i></a>'
                    + '</div>'

                $('.page-interests')[0].innerHTML += str;
                $("#tags").val("");

                checkPrefList();

            });


            function httpGetAsync(theUrl, callback) {
                var xmlHttp = new XMLHttpRequest();
                xmlHttp.onreadystatechange = function () {
                    if (xmlHttp.readyState == 4 && xmlHttp.status == 200)
                        callback(xmlHttp.responseText);
                }
                xmlHttp.open("GET", theUrl, true); // true for asynchronous 
                xmlHttp.send(null);
            }

            $('#addBTN2').on('click', function () {
                if ($("#selects").val() == "Suggested categories...") {
                    alert("Please choose a category");
                    return;
                }


                if ($("#selects").val().trim() == "") {
                    alert("Please choose a category");
                    return;
                }

                if (checkIfExist($("#selects").val().replace(/\W/g, ''))) {
                    alert("exist");
                    return;
                }

                var str = '<div id="' + $("#selects").val().replace(/\W/g, '') + '">' + $("#selects").val()
                    + '<a href="#" class="itemDelete"><i class="material-icons">delete</i></a>'
                    + '</div>';

                $('.page-interests')[0].innerHTML += str;


                printCategories(cat);
            });

            $.ajax({ // ajax call starts
                url: urlDomain + 'WebService.asmx/GetCategories',   // server side web service method
                //data: dataString,                          // the parameters sent to the server
                type: 'POST',                              // can be also GET
                dataType: 'json',                          // expecting JSON datatype from the server
                contentType: 'application/json; charset = utf-8', // sent to the server
                success: successGetCategoriesCB,                // data.d id the Variable data contains the data we get from serverside
                error: errorGetCategoriesCB
            }); // end of ajax call

            function successGetCategoriesCB(results) {
                var results = $.parseJSON(results.d);

                cat = results;

                printCategories(results);
            }

            function errorGetCategoriesCB(e) {
                //alert("I caught the exception : failed in GetCategories \n The exception message is : " + e.responseText);
            }

            function printCategories(results) {
                $('#selects').empty();
                var str = '<option>Suggested categories...</option>';
                for (var i = 0; i < results.length; i++) {

                    if (checkIfExist(results[i].replace(/\W/g, ''))) {
                        continue;
                    }

                    str += '<option>' + results[i] + '</option>';

                }
                $('#selects').append(str);

                checkPrefList();
            }

            $('#pi').on('click', '.itemDelete', function () {

                var x = $(this).parent()[0].id;

                $('#' + x).animate({ left: '500px' }, function () {

                    $(this).remove();
                    printCategories(cat);
                });

            });

            $('#forwardBTN1').on('click', function () {

                var catArr = [];

                for (var i = 0; i < $("#pi").children().length; i++) {

                    var a = $("#pi").children()[i].innerHTML;
                    var b = a.substring(0, a.indexOf("<"));

                    catArr.push(b);
                }

                var request = {
                    arr: catArr,
                    IMEI: localStorage.uuid
                }

                updateCategories(request);
            });

            function updateCategories(request) {


                var dataString = JSON.stringify(request);


                $.ajax({ // ajax call starts
                    url: urlDomain + 'WebService.asmx/UpdateCategories',   // server side web service method
                    data: dataString,                          // the parameters sent to the server
                    type: 'POST',                              // can be also GET
                    dataType: 'json',                          // expecting JSON datatype from the server
                    contentType: 'application/json; charset = utf-8', // sent to the server
                    success: successupdateCategoriesCB,                // data.d id the Variable data contains the data we get from serverside
                    error: errorupdateCategoriesCB
                }); // end of ajax call

                function successupdateCategoriesCB(results) {

                    checkPrefList();
                }

                function errorupdateCategoriesCB(e) {
                    //alert("I caught the exception : failed in updateCategories \n The exception message is : " + e.responseText);
                }
            }

            function checkPrefList() {
                if ($("#pi").children().length == 0) {
                    $('#forwardBTN1').hide();
                }
                else {
                    $('#forwardBTN1').show();
                }
            }
        }

        if (window.location.href.toString().indexOf('pref1a.html') != -1) {
            
            $("#burgerMenu").hide();

            cat = [];

            var request = {
                IMEI: localStorage.uuid
            }

            checkUser3(request);


            function checkUser3(request) {

                var dataString = JSON.stringify(request);

                $.ajax({ // ajax call starts
                    url: urlDomain + 'WebService.asmx/checkUser',   // server side web service method
                    data: dataString,                          // the parameters sent to the server
                    type: 'POST',                              // can be also GET
                    dataType: 'json',                          // expecting JSON datatype from the server
                    contentType: 'application/json; charset = utf-8', // sent to the server
                    success: checkUserSCB3,                // data.d id the Variable data contains the data we get from serverside
                    error: checkUserECB3
                }); // end of ajax call

            }

            function checkUserSCB3(results) {

                var results = $.parseJSON(results.d);

                userPref = results;

                if (userPref.Categories == null || userPref.Categories.length == 0) {
                    $('#forwardBTN1a').hide;
                }

                printCategories2();
            }

            function checkUserECB3(e) {
                //alert("I caught the exception : failed in checkUser \n The exception message is : " + e.responseText);

            }


            function printCategories2() {

                //var category = selector.eq(i)[0].id.slice(0, -1);


                for (var j = 0; j < userPref.Categories.length; j++) {

                    var select = userPref.Categories[j].Name;

                    if (select == 'Science and technology') {
                        select = 'ScinTech';
                    }

                    $('#' + select + 'C').eq(0).addClass('categoryActive');

                }

                checkPrefList2();
            }

            $('.category').on('click', function () {

                if ($(this).hasClass('categoryActive')) {

                    $(this).removeClass('categoryActive').addClass('categoryNotActive');

                }
                else {

                    $(this).removeClass('categoryNotActive').addClass('categoryActive');
                }

                checkPrefList2();

            });

            $('#forwardBTN1a').on('click', function () {

                showLoading();

                var catArr = [];

                var selector = $("#categoryContainer .categoryActive");

                for (var i = 0; i < selector.length; i++) {

                    var a = selector[i].innerHTML;

                    var b = a.substring(0, a.indexOf("<"));

                    if (b == 'Sci &amp; Tech') {
                        b = 'Science and technology';
                    }

                    catArr.push(b);
                }

                var request = {
                    arr: catArr,
                    IMEI: localStorage.uuid
                }

                updateCategories2(request);
            });

            function updateCategories2(request) {


                var dataString = JSON.stringify(request);


                $.ajax({ // ajax call starts
                    url: urlDomain + 'WebService.asmx/UpdateCategories',   // server side web service method
                    data: dataString,                          // the parameters sent to the server
                    type: 'POST',                              // can be also GET
                    dataType: 'json',                          // expecting JSON datatype from the server
                    contentType: 'application/json; charset = utf-8', // sent to the server
                    success: successupdateCategoriesCB2,                // data.d id the Variable data contains the data we get from serverside
                    error: errorupdateCategoriesCB2
                }); // end of ajax call

            }

            function successupdateCategoriesCB2(results) {

                localStorage.removeItem('articles');
                window.location.replace('profile.html');

            }

            function errorupdateCategoriesCB2(e) {
                //alert("I caught the exception : failed in updateCategories \n The exception message is : " + e.responseText);
            }

            function checkPrefList2() {
                if ($("#categoryContainer .categoryActive").length == 0) {
                    $('#forwardBTN1a').hide();
                }
                else {
                    $('#forwardBTN1a').show();
                }
            }
        }

        if (window.location.href.toString().indexOf('pref2.html') != -1) {

            userPref = $.parseJSON(localStorage.userPref);


            $("#burgerMenu").hide();

            document.getElementById("myonoffswitch-1").checked = userPref.ArticlePush;

            $('#artPerDay').val(userPref.ArticlesPerDay);
            $('#rangevalue2').val(userPref.ArticlesPerDay);


            document.getElementById("myonoffswitch-2").checked = userPref.LocationPush;
            document.getElementById("myonoffswitch-3").checked = userPref.PhotoPush;

            var hours = new Date(parseInt(userPref.PhotoPushTime.replace("/Date(", ""))).getHours();
            $('#photoTime').val(hours);
            $('#rangevalue3').val(hours);

            changeSwitches();

            $('#myonoffswitch-1').change(function () {
                changeSwitches();
            });

            $('#myonoffswitch-3').change(function () {
                changeSwitches();
            });

            $('#forwardBTN2').on('click', function () {

                var articleBOOL1 = $('#myonoffswitch-1').is(':checked');
                var articleQUAN1 = $('#artPerDay').val();

                var aroundBOOL1 = $('#myonoffswitch-2').is(':checked');

                var photoBOOL1 = $('#myonoffswitch-3').is(':checked');
                var photoTIME1 = $('#photoTime').val();

                var uid = parseInt(localStorage.Id);

                var request = {
                    articleBOOL: articleBOOL1,
                    articleQUAN: articleQUAN1,
                    aroundBOOL: aroundBOOL1,
                    photoBOOL: photoBOOL1,
                    photoTIME: photoTIME1,
                    userID: uid
                }

                updateUserPref(request);

            });

            function updateUserPref(request) {

                var dataString = JSON.stringify(request);


                $.ajax({ // ajax call starts
                    url: urlDomain + 'WebService.asmx/UpdateUserPrefs',   // server side web service method
                    data: dataString,                          // the parameters sent to the server
                    type: 'POST',                              // can be also GET
                    dataType: 'json',                          // expecting JSON datatype from the server
                    contentType: 'application/json; charset = utf-8', // sent to the server
                    success: successUpdateUserPrefCB,                // data.d id the Variable data contains the data we get from serverside
                    error: errorUpdateUserPrefsCB
                }); // end of ajax call

            }

            function successUpdateUserPrefCB(results) {

                localStorage.removeItem('articles');

                window.location.replace('profile.html');

            }

            function errorUpdateUserPrefsCB(e) {
                //alert("I caught the exception : failed in UpdateUserPrefs \n The exception message is : " + e.responseText);
            }

            function changeSwitches() {
                if ($('#myonoffswitch-1').is(':checked')) {
                    document.getElementById('artPerDay').disabled = false;

                    document.getElementById('artPerDay').value = 5;
                    $('#rangevalue2').val(5);
                }
                else {
                    document.getElementById('artPerDay').disabled = true;

                    document.getElementById('artPerDay').value = 0;
                    $('#rangevalue2').val(0);
                }

                if ($('#myonoffswitch-3').is(':checked')) {
                    document.getElementById('photoTime').disabled = false;
                }
                else {
                    document.getElementById('photoTime').disabled = true;

                }
            }


        }

        if (window.location.href.toString().indexOf('index.html') != -1) {

            //testing localy (no deviceready event)
            //var request = {
            //    IMEI: localStorage.uuid
            //}

            //checkUser2(request);

        }

        if (window.location.href.toString().indexOf('aroundme.html') != -1) {


            var lat = parseFloat(localStorage.lastLAT);
            var lng = parseFloat(localStorage.lastLNG);

            map = new google.maps.Map(document.getElementById('map'), {
                zoom: 14,
                center: new google.maps.LatLng(lat, lng),
                mapTypeId: 'terrain'
            });


            initAutocomplete();


            $('#myLocationBTN').on('click', function () {
                closeAllInfoWindows();
                getMyPosition();
            });


            $('#newLocationBTN').on('click', function () {
                closeAllInfoWindows();
                getNewWiki();
            });

            if (typeof fromAroundArticle === 'undefined') {
                getMyPosition();
            }
            else {
                getNewWiki();
            }

        }

        if (window.location.href.toString().indexOf('articlearound.html') != -1) {


            getArticleById(localStorage.lastPageid);
            fromAroundArticle = true;

        }

        function initAutocomplete() {


            // Create the search box and link it to the UI element.
            var input = document.getElementById('pac-input');
            var searchBox = new google.maps.places.SearchBox(input);
            map.controls[google.maps.ControlPosition.TOP_LEFT].push(input);

            // Bias the SearchBox results towards current map's viewport.
            map.addListener('bounds_changed', function () {
                searchBox.setBounds(map.getBounds());
            });

            // Listen for the event fired when the user selects a prediction and jump to it
            searchBox.addListener('places_changed', function () {
                var places = searchBox.getPlaces();

                if (places.length == 0) {
                    return;
                }

                // For each place, get the icon, name and location.
                var bounds = new google.maps.LatLngBounds();
                places.forEach(function (place) {

                    if (place.geometry.viewport) {
                        // Only geocodes have viewport.
                        bounds.union(place.geometry.viewport);
                    } else {
                        bounds.extend(place.geometry.location);
                    }
                });
                map.fitBounds(bounds);


                fromSearchBox = true;
                getNewWiki();
            });
        }


        function checkUser2(request) {
            var dataString = JSON.stringify(request);

            $.ajax({ // ajax call starts
                url: urlDomain + 'WebService.asmx/checkUser',   // server side web service method
                data: dataString,                          // the parameters sent to the server
                type: 'POST',                              // can be also GET
                dataType: 'json',                          // expecting JSON datatype from the server
                contentType: 'application/json; charset = utf-8', // sent to the server
                success: checkUserSCB2,                // data.d id the Variable data contains the data we get from serverside
                error: checkUserECB2
            }); // end of ajax call
        }

        function checkUserSCB2(results) {

            var results = $.parseJSON(results.d);

            userPref = results;

            localStorage.userPref = JSON.stringify(userPref);

            localStorage.Id = userPref.Id;

            setTimeout(function () {
                if (userPref.Categories == null || userPref.Categories.length == 0) {
                    window.location.replace("pref1a.html");

                }
                else {

                    window.location.replace("profile.html");
                }
                splashHandle = false;
            }, 1000);
        }

        function checkUserECB2(e) {
            //alert("I caught the exception : failed in checkUser2 \n The exception message is : " + e.responseText);

        }



        function updateUserRegId(request) {
            var dataString = JSON.stringify(request);

            $.ajax({ // ajax call starts
                url: urlDomain + 'WebService.asmx/updateUserRegId',   // server side web service method
                data: dataString,                          // the parameters sent to the server
                type: 'POST',                              // can be also GET
                dataType: 'json',                          // expecting JSON datatype from the server
                contentType: 'application/json; charset = utf-8', // sent to the server
                success: updateUserRegIdSCB,                // data.d id the Variable data contains the data we get from serverside
                error: updateUserRegIdECB
            }); // end of ajax call
        }

        function updateUserRegIdSCB(results) {

        }

        function updateUserRegIdECB(e) {
            //alert("I caught the exception : failed in updateUserRegId \n The exception message is : " + e.responseText);

        }



        if (window.location.href.toString().indexOf('profile.html') != -1) {

            $('.lodingGameText').hide();

            if (typeof searchThread !== 'undefined') {
                clearInterval(searchThread);
                searchThread = false;
            }

            articles = [];
            goSearch();


            $('.back-to-top-badge').hide();
            getStats();


            var request = {
                IMEI: localStorage.uuid,
                regId: localStorage.RegId || "test"
            }

            //todo - change it when deploying to app store
            //updateUserRegId(request);
        }

        function getStats() {
            var uid = parseInt(localStorage.Id);

            var request = {
                userId: uid
            }
            getProfile(request);
        }

        function getProfile(request) {

            var dataString = JSON.stringify(request);

            $.ajax({ // ajax call starts
                url: urlDomain + 'WebService.asmx/getProfile',   // server side web service method
                data: dataString,                          // the parameters sent to the server
                type: 'POST',                              // can be also GET
                dataType: 'json',                          // expecting JSON datatype from the server
                contentType: 'application/json; charset = utf-8', // sent to the server
                success: successGetProfileCB,                // data.d id the Variable data contains the data we get from serverside
                error: errorGetProfileCB
            }); // end of ajax call

        }


        function successGetProfileCB(data) {
            profile = $.parseJSON(data.d);

            var score = profile.Score;
            var readingSum = profile.ReadingSum;

            $('#scorePH').html("Total Score: " + score);
            $('#readingsPH').html("Articles Read: " + readingSum);


            var categories = profile.Categories;

            categories.sort(function (a, b) {
                return b.CategoryId - a.CategoryId;
            })

            var labelsArr = [];
            var dataArr = [];

            var len = categories.length;
            if (len > 5) {
                len = 6;
            }

            for (var i = 0; i < len; i++) {
                labelsArr.push(categories[i].Name);
                dataArr.push(categories[i].CategoryId);
            }

            var ctx = document.getElementById("myChart").getContext('2d');

            var myChart = new Chart(ctx, {
                type: 'pie',
                data: {
                    labels: labelsArr,
                    datasets: [{
                        label: '# of Votes',
                        data: dataArr,
                        backgroundColor: [
                            'rgba(255,138,34, 1)',
                            'rgba(246,66,66, 1)',
                            'rgba(186,38,68,1)',
                            'rgba(73,169,166, 1)',
                            'rgba(163,232,220, 1)',
                            'rgba(87,96,111, 1)'
                        ],
                        borderColor: [
                            'rgba(255,138,34, 1)',
                            'rgba(246,66,66, 1)',
                            'rgba(186,38,68,1)',
                            'rgba(73,169,166, 1)',
                            'rgba(163,232,220, 1)',
                            'rgba(87,96,111, 1)'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    tooltips: {
                        enabled: true
                    }
                }
            });
        }

        function errorGetProfileCB(e) {
            //alert("error in getProfile: " + e.responseText);
        }

        if (window.location.href.toString().indexOf('ranking.html') != -1) {

            getRanks();

        }

        function getRanks() {
            var uid = parseInt(localStorage.Id);

            var request = {
                userId: uid
            }
            getRanking(request);
        }

        function getRanking(request) {

            var dataString = JSON.stringify(request);

            $.ajax({ // ajax call starts
                url: urlDomain + 'WebService.asmx/getRanking',   // server side web service method
                data: dataString,                          // the parameters sent to the server
                type: 'POST',                              // can be also GET
                dataType: 'json',                          // expecting JSON datatype from the server
                contentType: 'application/json; charset = utf-8', // sent to the server
                success: successGetRankingCB,                // data.d id the Variable data contains the data we get from serverside
                error: errorGetRankingCB
            }); // end of ajax call

        }

        function successGetRankingCB(data) {
            var ranks = $.parseJSON(data.d);
            var uid = parseInt(localStorage.Id);
            var str = "";
            var name = 0;
            for (var i = 0; i < ranks.length; i++) {
                if (ranks[i].Id == uid) {
                    name = "Me";
                }
                else {
                    name = "Guest" + ranks[i].Id;
                }


                var medal = '<img src="images/';
                var score = ranks[i].Score;


                switch (true) {
                    case (score < 100):
                        medal += 'bronze_medal.png" />';
                        break;
                    case (score < 500):
                        medal += 'bronze_medal.png" />';
                        break;
                    case (score < 1000):
                        medal += 'silver_medal.png" />';
                        break;
                    case (score < 10000):
                        medal += 'gold_medal.png" />';
                        break;
                }

                if (i < 3) {
                    var rankColor = 'style="background:';
                    switch (i + 1) {
                        case 1:
                            rankColor += 'gold"';
                            break;
                        case 2:
                            rankColor += 'silver"';
                            break;
                        case 3:
                            rankColor += '#db7"';
                            break;
                    }

                    str = createRow(name, i, medal, ranks, str);
                }
                else {
                    str = createRow(name, i, medal, ranks, str);
                }

            }

            $("#ranking").append(str);
        }

        function createRow(name, i, medal, ranks, str) {
            if (name == "Me") {

                str += '<tr><td class="meRank">' + (i + 1) + '.</td>  <td class="meRank">' + name + '</td>  <td class="medal meRank">' + medal + '</td>  <td class="meRank">' + ranks[i].Score + "</td></tr>";
            }
            else {
                str += "<tr><td>" + (i + 1) + ".</td>  <td>" + name + '</td>  <td class="medal">' + medal + '</td>  <td>' + ranks[i].Score + '</td></tr>';
            }
            return str;
        }

        function errorGetRankingCB(e) {
            //alert("Error in errorGetRankingCB: " + e.responseText)
        }
        //Activate Menu
        //$('.footer-menu-open').click(function(){
        //    $('.footer-menu-open').addClass('remove-menu');
        //    $('.page-menu, .footer-menu-background, .footer-menu-tap-close, .footer-menu-close').removeClass('remove-menu');
        //    $('.page-menu .footer-menu-wrapper').removeClass('remove-wrapper');
        //    $($('.page-menu .footer-menu .footer-menu-item').get().reverse()).each(function(i){
        //        var row = $(this);
        //        setTimeout(function() {
        //            row.toggleClass('remove-menu');
        //        }, 20*i);
        //    });
        //});   

        //$('.footer-menu-item').click(function(){
        //    $('.footer-menu-open').removeClass('remove-menu');
        //    $('.footer-menu-close, .footer-menu-tap-close').addClass('remove-menu');
        //    $('.page-menu .footer-menu .footer-menu-item').each(function(i){
        //        var row = $(this);
        //        setTimeout(function() {
        //            row.toggleClass('remove-menu');
        //        }, 20*i);
        //    });
        //    var numItems = $('.page-menu .footer-menu .footer-menu-item').length
        //    setTimeout(function() {
        //        $('.footer-menu-background').addClass('remove-menu');
        //        $('.footer-menu-wrapper').addClass('remove-wrapper');
        //        $('.page-menu').addClass('remove-menu');
        //    }, numItems*20); //-50
        //});

        //Close Menu
        //$('.footer-menu-close, .footer-menu-tap-close').click(function(){
        //    $('.footer-menu-open').removeClass('remove-menu');
        //    $('.footer-menu-close, .footer-menu-tap-close').addClass('remove-menu');
        //    $('.page-menu .footer-menu .footer-menu-item').each(function(i){
        //        var row = $(this);
        //        setTimeout(function() {
        //            row.toggleClass('remove-menu');
        //        }, 20*i);
        //    });
        //    var numItems = $('.page-menu .footer-menu .footer-menu-item').length
        //    setTimeout(function() {
        //        $('.footer-menu-background').addClass('remove-menu');
        //        $('.footer-menu-wrapper').addClass('remove-wrapper');
        //        $('.page-menu').addClass('remove-menu');
        //    }, numItems*20); //-50
        //});      

        //Menu Size 
        function calculate_menu() {
            //Srolling menu to bottom position
            var scroll_footer = $('.footer-menu-scroll').height() + 100
            $('.footer-menu-scroll').animate({ scrollTop: scroll_footer }, 0);
            var menu_screen_width = $(window).width();
            var menu_screen_height = $(window).height();
            $('.footer-menu').css('height', menu_screen_height);
            $('.footer-menu').css('width', menu_screen_width);
        };

        $(window).resize(function () {
            calculate_menu();
        });
        calculate_menu();

        //Sidebar Dimensions Go here 
        var sidebar_width = 250;
        var sidebar_shadow_correction = 280; /*Add 30 Pixels to your sidebar width*/
        var sidebar_form_width = sidebar_width - 40;  /*This calculates the form size automatically*/

        $('.sidebar-left, .sidebar-right').css('width', sidebar_width);
        $('.sidebar-form').css('width', sidebar_form_width);

        $(".sidebar-left").css({
            "transform": "translateX(" + sidebar_shadow_correction * (-1) + "px)",
            "-webkit-transform": "translateX(" + sidebar_shadow_correction * (-1) + "px)",
            "-moz-transform": "translateX(" + sidebar_shadow_correction * (-1) + "px)",
            "-o-transform": "translateX(" + sidebar_shadow_correction * (-1) + "px)",
            "-ms-transform": "translateX(" + sidebar_shadow_correction * (-1) + "px)"
        });
        $(".sidebar-right").css({
            "transform": "translateX(" + sidebar_shadow_correction * (1) + "px)",
            "-webkit-transform": "translateX(" + sidebar_shadow_correction * (1) + "px)",
            "-moz-transform": "translateX(" + sidebar_shadow_correction * (1) + "px)",
            "-o-transform": "translateX(" + sidebar_shadow_correction * (1) + "px)",
            "-ms-transform": "translateX(" + sidebar_shadow_correction * (1) + "px)"
        });
        if ($('.sidebar-left').hasClass('sidebar-dark')) {
            $('body').css('background-color', '#0c1117');
        }

        if ($('.sidebar-left').hasClass('sidebar-light')) {
            $('body').css('background-color', '#FFFFFF');
        }

        //Sidebar Settings
        $('.open-left-sidebar').click(function () {
            $('.sidebar-left').addClass('active-sidebar-box');
            $('.sidebar-right').removeClass('active-sidebar-box');
            $('.sidebar-tap-close').addClass('active-tap-close');
            $('.back-to-top-badge').removeClass('back-to-top-badge-visible');
            return false;
        });

        //Activate this to animate to an active submenu
        if ($('.submenu').hasClass('active-submenu')) {
            //$('.sidebar-scroll').animate({
            //scrollTop: $('.active-submenu').offset().top - 60
            //}, 250);
        }

        $('.open-right-sidebar').click(function () {
            $('.sidebar-right').addClass('active-sidebar-box');
            $('.sidebar-left').removeClass('active-sidebar-box');
            $('.sidebar-tap-close').addClass('active-tap-close');
            $('.back-to-top-badge').removeClass('back-to-top-badge-visible');
            return false;
        });

        $('.open-submenu').click(function () {
            $(this).toggleClass('active-item');
            $(this).parent().find('.submenu').removeClass('sub-0');
            var total_submenu_items = $(this).parent().find('.submenu').children().length;
            $(this).parent().find('.submenu').toggleClass('sub-' + total_submenu_items);
            return false;
        });

        if ($('.submenu').hasClass('active-submenu')) {
            var total_submenu_items = $('.active-submenu').children().length;
            $('.active-submenu').toggleClass('sub-' + total_submenu_items);
        }

        $('.has-submenu').each(function () {
            var count_menus = $(this).find('.submenu').children().length;
            $(this).find('.menu-number').text(count_menus);
        });

        $(function () {

            if ($("#loading:visible").length == 1 || window.location.href.toString().indexOf('pref')) {
                return;
            }

            $("#page-content").swipe({
                swipeRight: function (event, direction, distance, duration, fingerCount) {
                    $('.sidebar-left').addClass('active-sidebar-box');
                    $('.sidebar-right').removeClass('active-sidebar-box');
                    $('.sidebar-tap-close').addClass('active-tap-close');
                    $('.back-to-top-badge').removeClass('back-to-top-badge-visible');
                },
                swipeLeft: function (event, direction, distance, duration, fingerCount) {
                    //$('.sidebar-right').addClass('active-sidebar-box');
                    //$('.sidebar-left').removeClass('active-sidebar-box');
                    //$('.sidebar-tap-close').addClass('active-tap-close');
                    //$('.back-to-top-badge').removeClass('back-to-top-badge-visible');
                },
                threshold: 50
            });
        });

        $(function () {
            $(".sidebar-left").swipe({
                swipeLeft: function (event, direction, distance, duration, fingerCount) {
                    $('.material-menu').removeClass('opacity-out');
                    $('.sidebar-left, .sidebar-right').removeClass('active-sidebar-box');
                    $('.sidebar-tap-close').removeClass('active-tap-close');
                },
                threshold: 50
            });
        });

        $(function () {
            $(".sidebar-right").swipe({
                swipeRight: function (event, direction, distance, duration, fingerCount) {
                    $('.sidebar-left, .sidebar-right').removeClass('active-sidebar-box');
                    $('.sidebar-tap-close').removeClass('active-tap-close');
                },
                threshold: 50
            });
        });

        $('.sidebar-tap-close, .close-sidebar').click(function () { //
            $('.material-menu').removeClass('opacity-out');
            $('.sidebar-left, .sidebar-right').removeClass('active-sidebar-box');
            $('.sidebar-tap-close').removeClass('active-tap-close');
            $("#page-content, .header, .footer-menu").css({
                "transform": "translateX(0px)",
                "-webkit-transform": "translateX(0px)",
                "-moz-transform": "translateX(0px)",
                "-o-transform": "translateX(0px)",
                "-ms-transform": "translateX(0px)"
            });
            return false;
        });

        $('.sidebar-left .menu-item, .sidebar-right .menu-item').click(function () {
            if ($(this).hasClass('open-submenu')) {
                return;
            }
            $('.material-menu').removeClass('opacity-out');
            $('.sidebar-left, .sidebar-right').removeClass('active-sidebar-box');
            $('.sidebar-tap-close').removeClass('active-tap-close');
            $("#page-content, .header, .footer-menu").css({
                "transform": "translateX(0px)",
                "-webkit-transform": "translateX(0px)",
                "-moz-transform": "translateX(0px)",
                "-o-transform": "translateX(0px)",
                "-ms-transform": "translateX(0px)"
            });
        });


        $('tabs a').click(function () {
            preventDefault();
            return false;
        });

        //FastClick
        $(function () { FastClick.attach(document.body); });

        //Preload Image
        $(function () {
            $(".preload-image").lazyload({
                threshold: 4000,
                effect: "fadeIn",
                container: $("#page-content-scroll")
            });
        });

        $('.hide-notification').click(function () {
            $(this).parent().slideUp();
            return false;
        });
        $('.tap-hide').click(function () {
            $(this).slideUp();
            return false;
        });

        $('.activate-toggle').click(function () {
            $(this).parent().find('.toggle-content').slideToggle(250);
            $(this).parent().find('input').each(function () { this.checked = !this.checked; });
            $(this).parent().find('.toggle-45').toggleClass('rotate-45 color-red-dark');
            $(this).parent().find('.toggle-180').toggleClass('rotate-180 color-red-dark');
            return false;
        });

        $('.accordion-item').click(function () {
            $(this).find('.accordion-content').slideToggle(250);
            $(this).find('i').toggleClass('rotate-135 color-red-dark');
            return false;
        });

        $('.dropdown-toggle').click(function () {
            $(this).parent().find('.dropdown-content').slideToggle(250);
            $(this).find('i:last-child').toggleClass('rotate-135');
            return false;
        });

        //Portfolio Wide

        $('.portfolio-wide-caption a').click(function () {
            $(this).parent().parent().find('.portfolio-wide-content').slideToggle(250);
            return false;
        });

        //Detect if iOS WebApp Engaged and permit navigation without deploying Safari
        (function (a, b, c) { if (c in b && b[c]) { var d, e = a.location, f = /^(a|html)$/i; a.addEventListener("click", function (a) { d = a.target; while (!f.test(d.nodeName)) d = d.parentNode; "href" in d && (d.href.indexOf("http") || ~d.href.indexOf(e.host)) && (a.preventDefault(), e.href = d.href) }, !1) } })(document, window.navigator, "standalone")

        //Detecting Mobiles//
        var isMobile = {
            Android: function () { return navigator.userAgent.match(/Android/i); },
            BlackBerry: function () { return navigator.userAgent.match(/BlackBerry/i); },
            iOS: function () { return navigator.userAgent.match(/iPhone|iPad|iPod/i); },
            Opera: function () { return navigator.userAgent.match(/Opera Mini/i); },
            Windows: function () { return navigator.userAgent.match(/IEMobile/i); },
            any: function () { return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Opera() || isMobile.Windows()); }
        };

        if (!isMobile.any()) {
            $('.show-blackberry, .show-ios, .show-windows, .show-android').addClass('disabled');
            $('.show-no-detection').removeClass('disabled');
            $('#page-content-scroll').css('right', '0px');
        }
        if (isMobile.Android()) {
            //Status Bar Color for Android
            $('head').append('<meta name="theme-color" content="#000000"> />');
            $('.show-android').removeClass('disabled');
            $('.show-blackberry, .show-ios, .show-windows, .show-download').addClass('disabled');
            $('#page-content-scroll, .sidebar-scroll').css('right', '0px');
            $('.set-today').addClass('mobile-date-correction');
        }
        if (isMobile.BlackBerry()) {
            $('.show-blackberry').removeClass('disabled');
            $('.show-android, .show-ios, .show-windows, .show-download').addClass('disabled');
            $('#page-content-scroll, .sidebar-scroll').css('right', '0px');
        }
        if (isMobile.iOS()) {
            $('.show-ios').removeClass('disabled');
            $('.show-blackberry, .show-android, .show-windows, .show-download').addClass('disabled');
            $('#page-content-scroll, .sidebar-scroll').css('right', '0px');
            $('.set-today').addClass('mobile-date-correction');
        }
        if (isMobile.Windows()) {
            $('.show-windows').removeClass('disabled');
            $('.show-blackberry, .show-ios, .show-android, .show-download').addClass('disabled');
            $('#page-content-scroll, .sidebar-scroll').css('right', '0px');
        }

        //Galleries
        $(".gallery a, .show-gallery").swipebox();

        //Adaptive Folios
        $('.adaptive-one').click(function () {
            $('.portfolio-switch').removeClass('active-adaptive');
            $(this).addClass('active-adaptive');
            $('.portfolio-adaptive').removeClass('portfolio-adaptive-two portfolio-adaptive-three');
            $('.portfolio-adaptive').addClass('portfolio-adaptive-one');
            return false;
        });
        $('.adaptive-two').click(function () {
            $('.portfolio-switch').removeClass('active-adaptive');
            $(this).addClass('active-adaptive');
            $('.portfolio-adaptive').removeClass('portfolio-adaptive-one portfolio-adaptive-three');
            $('.portfolio-adaptive').addClass('portfolio-adaptive-two');
            return false;
        });
        $('.adaptive-three').click(function () {
            $('.portfolio-switch').removeClass('active-adaptive');
            $(this).addClass('active-adaptive');
            $('.portfolio-adaptive').removeClass('portfolio-adaptive-two portfolio-adaptive-one');
            $('.portfolio-adaptive').addClass('portfolio-adaptive-three');
            return false;
        });

        //Show Back To Home When Scrolling
        $('#page-content-scroll').on('scroll', function () {
            var total_scroll_height = $('#page-content-scroll')[0].scrollHeight
            var inside_header = ($(this).scrollTop() <= 100);
            var passed_header = ($(this).scrollTop() >= 0); //250
            var footer_reached = ($(this).scrollTop() >= (total_scroll_height - ($(window).height() + 100)));

            if (inside_header == true) {
                $('.back-to-top-badge').removeClass('back-to-top-badge-visible');
                $('.header').removeClass('header-active');
            } else if (passed_header == true) {
                $('.header').addClass('header-active');
                $('.back-to-top-badge').addClass('back-to-top-badge-visible');
            }
            if (footer_reached == true) {
                //$('.back-to-top-badge').removeClass('back-to-top-badge-visible');
            }
        });

        //Back to top Badge
        $('.back-to-top-badge, .back-to-top').click(function (e) {
            e.preventDefault();
            $('#page-content-scroll').animate({
                scrollTop: 0
            }, 250);
        });

        //Share Bottom

        //Bottom Share Fly-up    
        $('body').append('<div class="share-bottom-tap-close"></div>');
        $('.show-share-bottom, .show-share-box').click(function () {
            $('.share-bottom-tap-close').addClass('share-bottom-tap-close-active');
            $('.share-bottom').toggleClass('active-share-bottom');
            return false;
        });
        $('.close-share-bottom, .share-bottom-tap-close').click(function () {
            $('.share-bottom-tap-close').removeClass('share-bottom-tap-close-active');
            $('.share-bottom').removeClass('active-share-bottom');
            return false;
        });

        //Set inputs to today's date by adding class set-day
        var set_input_now = new Date();
        var set_input_month = (set_input_now.getMonth() + 1);
        var set_input_day = set_input_now.getDate();
        if (set_input_month < 10)
            set_input_month = "0" + set_input_month;
        if (set_input_day < 10)
            set_input_day = "0" + set_input_day;
        var set_input_today = set_input_now.getFullYear() + '-' + set_input_month + '-' + set_input_day;
        $('.set-today').val(set_input_today);

        //Countdown Timer
        $(function () { $('.countdown-class').countdown({ date: "June 7, 2087 15:03:26" }); });

        //Copyright Year 
        if ($("#copyright-year")[0]) { document.getElementById('copyright-year').appendChild(document.createTextNode(new Date().getFullYear())) }
        if ($("#copyright-year-sidebar")[0]) { document.getElementById('copyright-year-sidebar').appendChild(document.createTextNode(new Date().getFullYear())) }

        //Contact Form
        var formSubmitted = "false"; jQuery(document).ready(function (e) { function t(t, n) { formSubmitted = "true"; var r = e("#" + t).serialize(); e.post(e("#" + t).attr("action"), r, function (n) { e("#" + t).hide(); e("#formSuccessMessageWrap").fadeIn(500) }) } function n(n, r) { e(".formValidationError").hide(); e(".fieldHasError").removeClass("fieldHasError"); e("#" + n + " .requiredField").each(function (i) { if (e(this).val() == "" || e(this).val() == e(this).attr("data-dummy")) { e(this).val(e(this).attr("data-dummy")); e(this).focus(); e(this).addClass("fieldHasError"); e("#" + e(this).attr("id") + "Error").fadeIn(300); return false } if (e(this).hasClass("requiredEmailField")) { var s = /^([\w-\.]+@([\w-]+\.)+[\w-]{2,4})?$/; var o = "#" + e(this).attr("id"); if (!s.test(e(o).val())) { e(o).focus(); e(o).addClass("fieldHasError"); e(o + "Error2").fadeIn(300); return false } } if (formSubmitted == "false" && i == e("#" + n + " .requiredField").length - 1) { t(n, r) } }) } e("#formSuccessMessageWrap").hide(0); e(".formValidationError").fadeOut(0); e('input[type="text"], input[type="password"], textarea').focus(function () { if (e(this).val() == e(this).attr("data-dummy")) { e(this).val("") } }); e("input, textarea").blur(function () { if (e(this).val() == "") { e(this).val(e(this).attr("data-dummy")) } }); e("#contactSubmitButton").click(function () { n(e(this).attr("data-formId")); return false }) })

        // Image Sliders
        var pricing_table = new Swiper('.pricing-table-slider', {
            pagination: '.swiper-pagination',
            paginationClickable: true,
            slidesPerView: 3,
            nextButton: '.pricing-table-next',
            prevButton: '.pricing-table-prev',
            spaceBetween: 50,
            breakpoints: {
                1024: {
                    slidesPerView: 2,
                    spaceBetween: 40
                },
                768: {
                    slidesPerView: 2,
                    spaceBetween: 30
                },
                640: {
                    slidesPerView: 1,
                    spaceBetween: 20
                },
                320: {
                    slidesPerView: 1,
                    spaceBetween: 10
                }
            }
        });

        var swiper = new Swiper('.coverpage-cube', {
            pagination: '.coverpage-slider .swiper-pagination',
            paginationClickable: true,
            loop: true,
            effect: 'cube',
            grabCursor: true,
            cube: {
                shadow: true,
                slideShadows: true,
                shadowOffset: 20,
                shadowScale: 0.94
            }
        });

        var swiper_coverpage = new Swiper('.coverpage-classic', {
            onSlideChangeStart: check_class,
            pagination: '.coverpage-slider .swiper-pagination',
            nextButton: '.flashing-arrows-1',
            prevButton: '.flashing-arrows-2',
            paginationClickable: true
        });

        function check_class() {
            if (swiper_coverpage.activeIndex > 0 > swiper_coverpage.isEnd) {
                $('.mobileui-home-footer').addClass('show-mobileui-home-footer');
                $('.header-homescreen').addClass('show-mobileui-home-header');
            } else {
                $('.mobileui-home-footer').removeClass('show-mobileui-home-footer');
                $('.header-homescreen').removeClass('show-mobileui-home-header');
            }

        }

        var swiper_category_slider = new Swiper('.category-slider', {
            pagination: '.swiper-pagination',
            paginationClickable: true,
            slidesPerView: 5,
            spaceBetween: 20,
            breakpoints: {
                1024: {
                    slidesPerView: 6,
                    spaceBetween: 20
                },
                768: {
                    slidesPerView: 5,
                    spaceBetween: 10
                },
                640: {
                    slidesPerView: 3,
                    spaceBetween: 5
                },
                320: {
                    slidesPerView: 3,
                    spaceBetween: 5
                }
            }
        });

        var swiper_store_thumbnail_slider = new Swiper('.store-thumbnails', {
            pagination: '.swiper-pagination',
            paginationClickable: true,
            slidesPerView: 5,
            spaceBetween: 20,
            breakpoints: {
                1024: {
                    slidesPerView: 6,
                    spaceBetween: 20
                },
                768: {
                    slidesPerView: 5,
                    spaceBetween: 10
                },
                640: {
                    slidesPerView: 2,
                    spaceBetween: 5
                },
                320: {
                    slidesPerView: 2,
                    spaceBetween: 5
                }
            }
        });

        var swiper_store_thumbnail_slider = new Swiper('.home-round-slider', {
            pagination: '.swiper-pagination',
            paginationClickable: true,
            slidesPerView: 3,
            spaceBetween: 20,
            breakpoints: {
                1024: {
                    slidesPerView: 3,
                    spaceBetween: 20
                },
                1024: {
                    slidesPerView: 2,
                    spaceBetween: 20
                },
                768: {
                    slidesPerView: 2,
                    spaceBetween: 10
                },
                660: {
                    slidesPerView: 1,
                    spaceBetween: 5
                },
                320: {
                    slidesPerView: 1,
                    spaceBetween: 5
                }
            }
        });

        setTimeout(function () {
            var swiper_coverflow_thumbnails = new Swiper('.coverflow-thumbnails', {
                pagination: '.swiper-pagination',
                effect: 'coverflow',
                autoplay: 3000,
                autoplayDisableOnInteraction: false,
                spaceBetween: -30,
                loop: true,
                grabCursor: true,
                centeredSlides: true,
                slidesPerView: 'auto',
                coverflow: {
                    rotate: 35,
                    stretch: -50,
                    depth: -190,
                    modifier: 1,
                    slideShadows: true
                }
            });
        }, 300);


        var swiper_coverflow_slider = new Swiper('.coverflow-slider', {
            pagination: '.swiper-pagination',
            effect: 'coverflow',
            autoplay: 3000,
            autoplayDisableOnInteraction: false,
            loop: true,
            grabCursor: true,
            centeredSlides: true,
            slidesPerView: 'auto',
            coverflow: {
                rotate: 60,
                stretch: -60,
                depth: 400,
                modifier: 1,
                slideShadows: false
            }
        });

        var swiper_staff_slider = new Swiper('.staff-slider', {
            nextButton: '.next-staff-slider',
            prevButton: '.prev-staff-slider',
            autoplay: 5000,
            loop: true,
            autoplayDisableOnInteraction: false,
            slidesPerView: 3,
            spaceBetween: 20,
            breakpoints: {
                1024: {
                    slidesPerView: 3,
                    spaceBetween: 20
                },
                768: {
                    slidesPerView: 2,
                    spaceBetween: 10
                },
                640: {
                    slidesPerView: 1,
                    spaceBetween: 5
                }
            }
        });

        var expanding_slider = new Swiper('.expanding-slider', {
            autoplay: 3000,
            autoplayDisableOnInteraction: false,
            slidesPerView: 4,
            spaceBetween: 20,
            breakpoints: {
                1024: {
                    slidesPerView: 4,
                    spaceBetween: 20
                },
                768: {
                    slidesPerView: 3,
                    spaceBetween: 10
                },
                640: {
                    slidesPerView: 1,
                    spaceBetween: 5
                },
                0: {
                    slidesPerView: 1,
                    spaceBetween: 5
                }
            }
        });

        var swiper = new Swiper('.home-slider', { autoplay: 4000, loop: true });
        var swiper_news_slider = new Swiper('.news-slider', { autoplay: 4000, loop: true });
        var swiper_single_item = new Swiper('.single-item', { autoplay: 4000, loop: true });
        var swiper_quote_slider = new Swiper('.quote-slider', { autoplay: 4000, loop: true });
        var swiper_store_slider = new Swiper('.store-slider', { autoplay: 3000, loop: true });
        var swiper_store_slider2 = new Swiper('.store-slider-2', { autoplay: 3000, loop: true });
        var swiper_text_slider = new Swiper('.text-slider', { autoplay: 2000, loop: true });

        //Aligning Elements & Resize Handlers//

        function center_content() {
            var screen_width = $(window).width();
            var screen_height = $(window).height();
            var content_width = $('.content-center').outerWidth();
            var content_height = $('.content-center').outerHeight();
            var content_full_width = $('.page-fullscreen-content').outerWidth();
            var content_full_height = $('.page-fullscreen-content').outerHeight();

            var cover_center_height = $('.coverpage-center').outerHeight();
            var cover_center_width = $('.coverpage-center').outerWidth();

            $('.content-center').css({
                "left": "50%",
                "top": "50%",
                "margin-left": (content_width / 2) * (-1),
                "margin-top": (content_height / 2) * (-1)
            });

            $('.page-fullscreen').css({
                "width": screen_width,
                "height": screen_height
            });

            $('.page-fullscreen-content').css({
                "left": "50%",
                "top": "50%",
                "margin-left": (content_full_width / 2) * (-1),
                "margin-top": (content_full_height / 2) * (-1)
            });

            $('.coverpage-clear').css({
                "height": screen_height
            });

            $('.coverpage-center').css({
                "left": "50%",
                "top": "50%",
                "margin-left": (cover_center_width / 2) * (-1),
                "margin-top": (cover_center_height / 2) * (-1)
            });

            $('.map-fullscreen iframe').css('width', screen_width);
            $('.map-fullscreen iframe').css('height', screen_height);


            var mobileui_home = (screen_height - 100);


            $('.mobileui-home').css('height', mobileui_home);
            $('.mobileui-home-5 a').css('height', mobileui_home / 5);
            $('.mobileui-home-4 a').css('height', mobileui_home / 4);
            $('.mobileui-home-3 a').css('height', mobileui_home / 3);
        };

        center_content();
        $(window).resize(function () {
            center_content();
            calculate_lockscreen();
        });

        //Fullscreen Map
        $('.map-text, .overlay').click(function () {
            $('.map-text, .map-fullscreen .overlay').addClass('hide-map');
            $('.deactivate-map').removeClass('hide-map');
            return false;
        });
        $('.deactivate-map').click(function () {
            $('.map-text, .map-fullscreen .overlay').removeClass('hide-map');
            $('.deactivate-map').addClass('hide-map');
            return false;
        });

        //Classic Toggles
        $('.toggle-title').click(function () {
            $(this).parent().find('.toggle-content').slideToggle(250);
            $(this).find('i').toggleClass('rotate-toggle');
            return false;
        });

        //Checklist Item
        $('.checklist-item').click(function () {
            $(this).find('.fa-circle-o').toggle(250);
            $(this).find('strong').toggleClass('completed-checklist');
            $(this).find('.fa-check, .fa-times, .fa-check-circle-o, .fa-check-circle, .fa-times-circle, .fa-times-circle-o').toggle(250);
        });

        if ($('.checklist-item').hasClass('checklist-item-complete')) {
            $('.checklist-item-complete').find('.fa-circle-o').toggle(250);
            $('.checklist-item-complete').find('strong').toggleClass('completed-checklist');
            $('.checklist-item-complete').find('.fa-check, .fa-times, .fa-check-circle-o, .fa-check-circle, .fa-times-circle, .fa-times-circle-o').toggle(250);
        }

        //Tasklist Item
        $('.tasklist-incomplete').click(function () {
            $(this).removeClass('tasklist-incomplete');
            $(this).addClass('tasklist-completed');
            return false;
        });
        $('.tasklist-item').click(function () {
            $(this).toggleClass('tasklist-completed');
            return false;
        });

        //Interests
        $('.interest-box').click(function () {
            $(this).toggleClass('transparent-background');
            $(this).find('.interest-first-icon, .interest-second-icon').toggleClass('hide-interest-icon');
            return false;
        });

        //Loading Thumb Layout for News, 10 articles at a time
        $(function () {
            $(".thumb-layout-page a").slice(0, 5).show(); // select the first ten
            $(".load-more-thumbs").click(function (e) { // click event for load more
                e.preventDefault();
                $(".thumb-layout-page a:hidden").slice(0, 5).show(0); // select next 10 hidden divs and show them
                if ($(".thumb-layout-page a:hidden").length == 0) { // check if any hidden divs still exist
                    $(this).hide();
                }
            });
        });

        $(function () {
            $(".card-large-layout-page .card-large-layout").slice(0, 2).show(); // select the first ten
            $(".load-more-large-cards").click(function (e) { // click event for load more
                e.preventDefault();
                $(".card-large-layout-page .card-large-layout:hidden").slice(0, 2).show(0); // select next 10 hidden divs and show them
                if ($(".card-large-layout-page div:hidden").length == 0) { // check if any hidden divs still exist
                    $(this).hide();
                }
            });
        });

        $(function () {
            $(".card-small-layout-page .card-small-layout").slice(0, 3).show(); // select the first ten
            $(".load-more-small-cards").click(function (e) { // click event for load more
                e.preventDefault();
                $(".card-small-layout-page .card-small-layout:hidden").slice(0, 3).show(0); // select next 10 hidden divs and show them
                if ($(".card-small-layout-page a:hidden").length == 0) { // check if any hidden divs still exist
                    $(this).hide();
                }
            });
        });

        //News Tabs
        $('.activate-tab-1').click(function () {
            $('#tab-2, #tab-3').slideUp(250); $('#tab-1').slideDown(250);
            $('.home-tabs a').removeClass('active-home-tab');
            $('.activate-tab-1').addClass('active-home-tab');
            return false;
        });
        $('.activate-tab-2').click(function () {
            $('#tab-1, #tab-3').slideUp(250); $('#tab-2').slideDown(250);
            $('.home-tabs a').removeClass('active-home-tab');
            $('.activate-tab-2').addClass('active-home-tab');
            return false;
        });
        $('.activate-tab-3').click(function () {
            $('#tab-1, #tab-2').slideUp(250); $('#tab-3').slideDown(250);
            $('.home-tabs a').removeClass('active-home-tab');
            $('.activate-tab-3').addClass('active-home-tab');
            return false;
        });

        //Tabs
        $('ul.tabs li').click(function () {
            var tab_id = $(this).attr('data-tab');

            $(this).parent().parent().find('ul.tabs li').removeClass('current');
            $(this).parent().parent().find('.tab-content').removeClass('current');

            $(this).addClass('current');
            $("#" + tab_id).addClass('current');
        })

        //Store Cart Add / Substract Numbers
        $(function () {
            $('.add-qty').on('click', function () {
                var $qty = $(this).closest('div').find('.qty');
                var currentVal = parseInt($qty.val());
                if (!isNaN(currentVal)) {
                    $qty.val(currentVal + 1);
                }
                return false;
            });
            $('.substract-qty').on('click', function () {
                var $qty = $(this).closest('div').find('.qty');
                var currentVal = parseInt($qty.val());
                if (!isNaN(currentVal) && currentVal > 0) {
                    $qty.val(currentVal - 1);
                }
                return false;
            });
        });

        $('.remove-cart-item').click(function () {
            $(this).parent().parent().slideUp(250);
            return false;
        });

        //Mobile UI Controls//

        //Dial Screen
        $('.phone-pad-1').click(function () { var this_value = $('.mobileui-dialpad input').val(); $('.mobileui-dialpad input').val(this_value + '1'); });
        $('.phone-pad-2').click(function () { var this_value = $('.mobileui-dialpad input').val(); $('.mobileui-dialpad input').val(this_value + '2'); });
        $('.phone-pad-3').click(function () { var this_value = $('.mobileui-dialpad input').val(); $('.mobileui-dialpad input').val(this_value + '3'); });
        $('.phone-pad-4').click(function () { var this_value = $('.mobileui-dialpad input').val(); $('.mobileui-dialpad input').val(this_value + '4'); });
        $('.phone-pad-5').click(function () { var this_value = $('.mobileui-dialpad input').val(); $('.mobileui-dialpad input').val(this_value + '5'); });
        $('.phone-pad-6').click(function () { var this_value = $('.mobileui-dialpad input').val(); $('.mobileui-dialpad input').val(this_value + '6'); });
        $('.phone-pad-7').click(function () { var this_value = $('.mobileui-dialpad input').val(); $('.mobileui-dialpad input').val(this_value + '7'); });
        $('.phone-pad-8').click(function () { var this_value = $('.mobileui-dialpad input').val(); $('.mobileui-dialpad input').val(this_value + '8'); });
        $('.phone-pad-9').click(function () { var this_value = $('.mobileui-dialpad input').val(); $('.mobileui-dialpad input').val(this_value + '9'); });
        $('.phone-pad-0').click(function () { var this_value = $('.mobileui-dialpad input').val(); $('.mobileui-dialpad input').val(this_value + '0'); });
        $('.phone-pad-star').click(function () { var this_value = $('.mobileui-dialpad input').val(); $('.mobileui-dialpad input').val(this_value + '*'); });
        $('.phone-pad-hash').click(function () { var this_value = $('.mobileui-dialpad input').val(); $('.mobileui-dialpad input').val(this_value + '#'); });

        $('.call-dial').click(function () {
            $(this).toggleClass('bg-red-dark');
            $(this).find('i').toggleClass('rotate-135');
            $('.mobileui-dialpad-numbers').slideToggle(250);
            $('.mobileui-dialpad-controls').slideToggle(250);
        });


        //Lockscreen
        new Date($.now());
        var dt = new Date();
        var time = dt.getHours() + ":" + ("0" + dt.getMinutes()).substr(-2);;
        $(".mobileui-lockscreen-header h3").html(time);
        var monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
        var d = new Date();
        var today_day = d.getDate();
        var today_year = d.getFullYear();
        var dateString = today_day;
        var daysOfWeek = new Array('Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday');
        var today_weekday = daysOfWeek[new Date(dateString).getDay()];

        $(".mobileui-lockscreen-header p").html(today_weekday + ", " + today_day + "  " + monthNames[d.getMonth()] + "  " + today_year);

        function calculate_lockscreen() {
            var lock_height = $('.mobileui-lockscreen-header').height();
            var lock_button = $('.mobileui-lockscreen-home').height();
            var lock_window = $(window).height() - 0;
            var lock_total = lock_window - (lock_button + lock_height);
            $('.mobileui-lockscreen-notifications').css('height', lock_total - 160);
        };
        calculate_lockscreen();


    }//Init Template Function


    setTimeout(init_template, 0);//Activating all the plugins
    $(function () {
        'use strict';
        var options = {
            prefetch: false,
            cacheLength: 0,
            blacklist: '.default-link',
            forms: 'contactForm',
            onStart: {
                duration: 250, // Duration of our animation
                render: function ($container) {
                    // Add your CSS animation reversing class
                    $container.addClass('is-exiting');

                    // Restart your animation
                    smoothState.restartCSSAnimations();
                    $('.page-preloader').addClass('show-preloader');
                    $('#page-content, .header, .footer-menu-open').css({
                        "transform": "translateX(" + ($(window).width()) * (1) + "px)",
                        "-webkit-transform": "translateX(" + ($(window).width()) * (1) + "px)",
                        "-moz-transform": "translateX(" + ($(window).width()) * (1) + "px)",
                        "-o-transform": "translateX(" + ($(window).width()) * (1) + "px)",
                        "-ms-transform": "translateX(" + ($(window).width()) * (1) + "px)",
                        "transition": "all 250ms ease"
                    });

                }
            },
            onReady: {
                duration: 0,
                render: function ($container, $newContent) {
                    // Remove your CSS animation reversing class
                    $container.removeClass('is-exiting');

                    // Inject the new content
                    $container.html($newContent);
                    $('.page-preloader').addClass('show-preloader');
                    $('#page-content, .header, .footer-menu-open').css({
                        "transform": "translateX(" + ($(window).width()) * (-1) + "px)",
                        "-webkit-transform": "translateX(" + ($(window).width()) * (-1) + "px)",
                        "-moz-transform": "translateX(" + ($(window).width()) * (-1) + "px)",
                        "-o-transform": "translateX(" + ($(window).width()) * (-1) + "px)",
                        "-ms-transform": "translateX(" + ($(window).width()) * (-1) + "px)",
                        "transition": "all 0ms ease"
                    });

                }
            },

            onAfter: function ($container, $newContent) {
                setTimeout(init_template, 0)//Timeout required to properly initiate all JS Functions. 
                $('.page-preloader').removeClass('show-preloader');
                setTimeout(function () {
                    $('#page-content, .header, .footer-menu-open').css({
                        "transform": "translateX(" + 0 * (1) + "px)",
                        "-webkit-transform": "translateX(" + 0 * (1) + "px)",
                        "-moz-transform": "translateX(" + 0 * (1) + "px)",
                        "-o-transform": "translateX(" + 0 * (1) + "px)",
                        "-ms-transform": "translateX(" + 0 * (1) + "px)",
                        "transition": "all 250ms ease"
                    });
                }, 250);

            }
        };
        var smoothState = $('#page-transitions').smoothState(options).data('smoothState');
    });

});

