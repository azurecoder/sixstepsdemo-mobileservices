var SendGrid = require('sendgrid').SendGrid;

function insert(item, user, request) {
    // AUTHENTICATION Script - get the user id
    var uid = user.userId;
    twilioSms("performing add for " + item.length + " speaker(s) by " + uid);
    // TODO - perform any validation and security checks i.e have a table or list of authorised admins
    var speakersTable = tables.getTable('speakers');
    var speakers = item.speakers;
    var ids = new Array(speakers.length);
    var count = 0;
    speakers.forEach(function (speaker, index) {
        speakersTable.insert(speaker, {
            success: function () {
                // Todo: add an email send here
                sendEmail(uid, speaker.Name);
                // Todo: send the push notification here
                sendPushNotification(speaker, user);
                backupPhotos(speaker.PictureUrl);
                // keep a count of callbacks
                count++;
                // build a list of new ids - make sure
                // they go back in the right order
                ids[index] = speaker.Id;
                if (speakers.length === count) {
                    // we've finished all updates, 
                    // send response with new IDs
                    request.respond(200);
                }
            },

            error: function () {
                count++;
                console.log("user " + uid + " failed to add new speaker with name " + item.Name);
            }
        });
    });
}

/* This function is used to send an email when a user adds a new speaker
 * giving the originating user id */
function sendEmail(userid, speakerName) {
    var sendgrid = new SendGrid('<username>', '<password>');

    sendgrid.send({
        to: '<to email>',
        from: '<from email>',
        subject: 'New insert from into mobile services',
        text: 'A new speaker called ' + speakerName + ' has been added to the mobile service Speakers table by: ' + userid
    }, function (success, message) {
        if (!success) {
            console.error(message);
        }
    });
}
/* This function is used to send a text message via twilio */
function twilioSms(message) {
    var httpRequest = require('request');
    var account_sid = "<account sid>";
    var auth_token = "<account toke>";
    // Create the request body
    var body = "From=<your twilio number>&To=<your number>&Body=" + message;
    // Make the HTTP request to Twilio
    httpRequest.post({
        url: "https://" + account_sid + ":" + auth_token + "@api.twilio.com/2010-04-01/Accounts/" + account_sid + "/SMS/Messages.json",
        headers: { 'content-type': 'application/x-www-form-urlencoded' },
        body: body
    }, function (err, resp, body) {
        console.log(body);
    });
}

function getChannels(callback, user) {
    // this example loads all the channelUris from a table called Channel
    var sql = "SELECT ChannelUri FROM Channels WHERE UserId = ?";
    mssql.query(sql, [user.userId], {
        success: function (results) {
            callback(results);
        }
    });
}

// send the push notification
function sendPushNotification(item, user) {
    getChannels(function (results) {
        results.forEach(function (result) {
            push.wns.sendToastImageAndText04(result.ChannelUri, {
                text1: "New speaker added: " + item.Name,
                image1src: item.PictureUrl
            });
            push.wns.sendToastText04(result.ChannelUri, {
                text1: 'New speaker added'
            });
            push.wns.sendTileWideImageAndText01(result.ChannelUri, {
                text: item.Name,
                imageSrc: item.PictureUrl
            });
            push.wns.sendTileSquarePeekImageAndText01(result.ChannelUri, {
                image1src: item.PictureUrl,
                image1alt: item.Name,
                text1: 'Just added ',
                text2: 'another speaker',
                text3: 'called ',
                text4: item.Name
            });

            mssql.query("SELECT COUNT(*) FROM SPEAKERS", {
                success: function (count) {
                    // TODO: Update the badge here!!
                }
            });
            console.log("sent to channel " + result.ChannelUri);
        });
    }, user);

}

function backupPhotos(pictureUrl) {
    var azure = require('azure');
    var path = require('path');

    var origname = path.basename(pictureUrl);
    var copyname = path.basename(pictureUrl, '.jpg') + '.backup' + path.extname(pictureUrl);
    console.log('copying original blob called ' + origname + ' to ' + copyname);
    var blobService = azure.createBlobService("<account name>", "<account key>");
    blobService.copyBlob('eventphotos', origname, 'backups', copyname,
        function (error, blobresponse, response) {
            if (error !== null)
                console.error(error);
        });
}
