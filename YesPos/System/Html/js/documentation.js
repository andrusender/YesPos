Object.prototype.next = function () {
    var n = this;
    do n = n.nextSibling;
    while (n && n.nodeType != 1);
    return n;
}