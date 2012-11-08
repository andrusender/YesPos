function getNext(n) {    
    do n = n.nextSibling;
    while (n && n.nodeType != 1);
    return n;
}