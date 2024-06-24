function AutoResizeTextarea(textareaClassName) {

    let textareas = document.querySelectorAll(`.${textareaClassName}`);

    textareas.forEach((item) => {
        autoResize(item);

        item.oninput = () => {
            autoResize(item);
        }
    })
    function autoResize(textarea) {
        textarea.style.height = 'auto';
        textarea.style.height = textarea.scrollHeight + 'px';
    }
}
