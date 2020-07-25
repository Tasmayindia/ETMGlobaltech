function move (id)
{
    for (var i=1;i<=30;i++)
    {
        var e = document.getElementById(i);
        
        e.style.display = 'none';
//        if(i==1)
//        {
//            document.getElementById('t'+i).className="";
//        }
//        else
//        {
//            document.getElementById('t'+i).className="";
//        }
    }
    var t = document.getElementById(id);
//    document.getElementById('t'+id).className="";
    t.style.display = 'block';
    
}





function textboxMultilineMaxNumber(txt, maxLen) {
    try {
        if (txt.value.length > (maxLen - 1)) return false;
    } catch (e) {
    }
}






