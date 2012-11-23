function insert(item, user, request) {
    item.UserId = user.userId;
    var channels = tables.getTable("Channels");

    channels.where({ InstallationId: item.InstallationId, UserId: user.userId }).read({
        success: function (results) {
            if (results.length > 0) {
                // get the first record if we know we have more than 1
                var currentChannel = results[0];
                // check to see whether the channel ids match
                if (currentChannel.ChannelUri !== item.ChannelUri) {
                    currentChannel.ChannelUri = item.ChannelUri;
                    channels.update(currentChannel, {
                        success: function () {
                            request.respond(200, currentChannel);
                        }
                    });
                }
                else {
                    // return the original if there is no update needed
                    request.respond(200, currentChannel);
                }
            }
            else {
                // if we dont't have a channel let's add 1
                request.execute();
            }
        }
    });
}