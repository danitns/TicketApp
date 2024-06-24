$(document).on("click", ".print", function () {
    var parentDiv = $(this).closest('.modal-content');

    const section = $("section");
    const modalContentWrapper = $(this).closest('.modal-content-wrapper');
    const modalBody = parentDiv.detach();


    const content = $(".content").detach();
    section.append(modalBody);
    window.print();
    section.empty();
    section.append(content);
    modalContentWrapper.append(modalBody);
});