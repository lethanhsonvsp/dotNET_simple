window.exampleJsFunctions = {
    showAlert: function (message) {
        alert(message);
        return message;
    },
    promptUser: function (message) {
        return prompt(message, 'Type your name here');
    }
};
