let quillInstances = {};

window.QuillEditor = {
    init: function (elementId, readOnly = false) {
        if (quillInstances[elementId]) {
            return;
        }

        const toolbarOptions = [
            ['bold', 'italic', 'underline', 'strike'],
            ['blockquote', 'code-block'],
            [{ 'header': 1 }, { 'header': 2 }],
            [{ 'list': 'ordered' }, { 'list': 'bullet' }],
            [{ 'indent': '-1' }, { 'indent': '+1' }],
            [{ 'size': ['small', false, 'large', 'huge'] }],
            [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
            [{ 'color': [] }, { 'background': [] }],
            [{ 'align': [] }],
            ['link', 'image'],
            ['clean']
        ];

        quillInstances[elementId] = new Quill(`#${elementId}`, {
            theme: 'snow',
            modules: {
                toolbar: toolbarOptions
            },
            readOnly: readOnly,
            placeholder: 'Start writing your heart out...'
        });
    },

    getContent: function (elementId) {
        const quill = quillInstances[elementId];
        if (quill) {
            return quill.root.innerHTML;
        }
        return '';
    },

    setContent: function (elementId, htmlContent) {
        const quill = quillInstances[elementId];
        if (quill && htmlContent) {
            quill.root.innerHTML = htmlContent;
        }
    },

    getText: function (elementId) {
        const quill = quillInstances[elementId];
        if (quill) {
            return quill.getText();
        }
        return '';
    },

    destroy: function (elementId) {
        if (quillInstances[elementId]) {
            delete quillInstances[elementId];
        }
    }
};