function appendItemToHtmlContainer(item)
{
    let itemsContainer = document.getElementById("items_container");

    let htmlItem = `<div class="ls_item">
        ${item}
    <div>`;
    let template = document.createElement("template");
    template.innerHTML = htmlItem;

    itemsContainer.append(template.content.children[0]);
}

async function goToView(viewName, onLoadAction)
{
    const mainContainer = document.getElementById("main_container");

    try {
        const url = `views/${viewName}.html`;
        const response = await fetch(url);
        mainContainer.innerHTML = await response.text();
        
        if (onLoadAction !== undefined) {
            eval(onLoadAction);
        }

        sessionStorage.setItem('lastVisitedView', viewName);
    }
    catch (ex) {
        console.log("An error occurred while loading the content!");
        console.log(ex);
    }
}

async function menuItemOnClickHandler(event)
{
    const viewName = event.currentTarget.getAttribute("route");
    const action = event.currentTarget.getAttribute("onloadaction");
    goToView(viewName, action);
}

const menuItemsContainer = document.getElementById("menu_items_container");

for (const menuItem of menuItemsContainer.children) {
    menuItem.onclick = menuItemOnClickHandler;
}


window.onload = function() {
    let viewName = sessionStorage.getItem('lastVisitedView');
    if (viewName === null) {
        viewName = "home";
    }
    goToView(viewName);
}