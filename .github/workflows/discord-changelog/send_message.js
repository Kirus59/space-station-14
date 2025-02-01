const core = require('@actions/core');
const github = require('@actions/github');
const hookcord = require('hookcord');

const webhook_id = core.getInput('webhook_id');
const webhook_secret = core.getInput('webhook_secret');

if (webhook_id === "" || webhook_secret === "") return;

try {
    const body = github.context.payload.pull_request.body;
    let author = github.context.payload.pull_request.user.login;

    if (body === undefined || body === "") return;
    if (typeof author !== 'string' || author === undefined){
        author = "Неизвестно";
    }

    TrySendMessage(body, author);
}
catch (error) {
    core.setFailed(error.message);
}

function extractCL(text){
    if (text === null) return null;

    const clregex = /:cl:/g;

    let clIndexes = Array.from(text.matchAll(clregex));

    if (clIndexes.length === 0) return null;

    let clStrings = new Array();
    if (clIndexes.length > 1){
        for (let i = 0; i < clIndexes.length; i++){
            if (i != clIndexes.length - 1){
                clStrings[i] = text.substring(clIndexes[i].index, clIndexes[i + 1].index);
            }
            else{
                clStrings[i] = text.substring(clIndexes[i].index, text.length - 1);
            }
        }
    }
    else{
        clStrings[0] = text.substring(clIndexes[0].index, text.length - 1);
    }

    return clStrings;
}

function TrySendMessage(text, user){
    if (typeof text !== 'string' || typeof user !== 'string') {
        console.log(`Params in TrySendMessage are not string!`);
        return;
    };

    var clStrings = extractCL(text);
    if (clStrings.length <= 0 || clStrings === null){
        console.log(`Doesn't found any cl string!`);
        return;
    }

    for (cl of clStrings){
        const authorRegex = /(?<=:cl:).*/g;
        const infoRegex = /^-.*\w+:.*$/gm;

        let authors = user;
        let authorsStr = authorRegex.exec(cl)[0].trim();
        if (authorsStr !== null && authorsStr !== ""){
            authors = authorsStr;
        }
+
        console.log(`Authors: ${authors}`)

        let infoArray = new Array();
        let i = 0;
        let m;
        while ((m = infoRegex.exec(cl)) !== null) {
            if (m.index === infoRegex.lastIndex) {
                infoRegex.lastIndex++;
            }

            m.forEach((match, groupIndex) => {
                infoArray[i] = match;
                i++;
            });
        }

        if (infoArray === null){
            console.log(`Doesn't found any info string!`)
            continue;
        }

        console.log(infoArray.length);
        let infoText = "";
        for (let i = 0; i < infoArray.length; i++){
            let curInfo = infoArray[i];

            if (typeof curInfo !== 'string' ||
                curInfo === null ||
                curInfo === "")
                continue;

            const dashRegex = /.*\s(?=\w+:)/g;

            curInfo = curInfo.replaceAll(dashRegex, "");
            for (let [key, value] of ReplaceData){
                curInfo = curInfo.replaceAll(key, value);
            }

            console.log(curInfo);

            if (i != infoArray.length - 1){
                infoText += curInfo + "\n";
            }
            else{
                infoText += curInfo;
            }
        }

        if (infoText === ""){
            console.log(`doesn't generate infoText`.toUpperCase())
            continue;
        }

        var Hook = new hookcord.Hook()
            .login(id, secret)
            .setPayload({'embeds': [{
                "color": 14397510,
                'fields': [{
                'name': `Авторы: ${authors}`,
                'value': infoText
                }]
            }]})
            .fire()
            .then(response_object => {})
            .catch(error => {
                throw error;
        });
    }
}
