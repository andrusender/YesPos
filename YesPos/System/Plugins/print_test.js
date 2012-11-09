(function (window) {
    //setTimeout("code()", 2000);
    print_test();
    function print_test() {
        //alert(document.cookie);
        var installed_printers = window.external.get_system_printers();
        var html = "";
        for (var i = 0; i < installed_printers.length; i++) {
            html += "<input type='button' style='padding:5px;margin:5px;' value='PRINT BY [" + installed_printers[i] + "]' onclick='__yesposPrintBy(" + i + ")' />";
        }
        var el = document.createElement("div");
        el.style.position = "absolute";
        el.style.backgroundColor = "yellow";
        el.style.padding = 10;
        el.style.top = 0;
        el.style.right = 0;
        el.innerHTML = html;
        document.getElementsByTagName("body")[0].appendChild(el);
    }

    var printBy = function (p) {
        var printers = window.external.get_system_printers();
        var options = {
            printer: printers[p],
            marginLeft: 10,
            marginRight: 10,
            marginTop: 10,
            marginBottom: 10,
            paperWidth: 2100,
            paperHeight: 297
        };
        window.external.print(JSON.stringify(options));
    }
    window.__yesposPrintBy = printBy;
    //window.external.set_tray_icon_visible(true);
    //window.external.show_tray_baloon("xxx", "xxx", "", "1000", "alert('x')");
    //window.external.show_notification(JSON.stringify({content:"Hello",title:"world"}));
    //window.external.set_window_minimum_size(1, "yx3");
    //window.external.set_badge_text("9", "18", "#ffffff", "center"); //topLeft topRight bottomLeft bottomRight center
    //window.external.show_window_from_tray();
})(window);
