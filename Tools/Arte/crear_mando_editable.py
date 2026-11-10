"""Original controller artwork, layered Aseprite source and Unity exports.

Run with Python 3 and Pillow. No existing project images are read or modified.
The file writer follows https://github.com/aseprite/aseprite/blob/main/docs/ase-file-specs.md
"""
from pathlib import Path
import math
import struct
import zlib
import json
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/Controles"
EXPORT = ROOT / "Assets/CampusGlitch/Resources/CG/Controls"
SIZE = (512, 300)
LAYERS = ["01_Carcasa", "02_Empunaduras", "03_Panel_y_costuras", "04_Pozos_sticks", "05_Botones", "06_Simbolos", "07_Estado_luminoso", "08_Identidad_Campus"]
STATES = ["Reposo", "Seleccionado", "Pulsado", "Desconectado"]


def blank():
    return Image.new("RGBA", SIZE)


def curve(start, segments):
    points = [start]
    for a, b, end in segments:
        for step in range(1, 25):
            t = step / 24
            u = 1 - t
            points.append((u**3 * start[0] + 3*u*u*t*a[0] + 3*u*t*t*b[0] + t**3*end[0],
                           u**3 * start[1] + 3*u*u*t*a[1] + 3*u*t*t*b[1] + t**3*end[1]))
        start = end
    return points


def anatomy(family):
    return dict(left=(145, 120) if family == "Xbox" else (190, 198), right=(305, 195) if family == "Xbox" else (322, 198),
                dpad=(210, 195) if family == "Xbox" else (128, 119), face=(389, 126),
                view=(232, 116) if family == "Xbox" else (166, 78), menu=(280, 116) if family == "Xbox" else (346, 78))


def shell(family):
    if family == "Xbox":
        points = curve((126, 58), [((163, 48),(195, 55),(256, 54)),((317, 55),(349, 48),(386, 58)),
            ((427, 64),(451, 107),(460, 159)),((470, 205),(482, 258),(450, 280)),
            ((421, 301),(389, 268),(352, 222)),((322, 207),(290, 209),(256, 210)),
            ((222, 209),(190, 207),(160, 222)),((123, 268),(91, 301),(62, 280)),
            ((30, 258),(42, 205),(52, 159)),((61, 107),(85, 64),(126, 58))])
    else:
        points = curve((119, 55), [((169, 42),(211, 55),(256, 52)),((301, 55),(343, 42),(393, 55)),
            ((434, 67),(450, 105),(459, 152)),((470, 199),(489, 262),(456, 281)),
            ((423, 295),(392, 267),(361, 215)),((330, 207),(297, 218),(256, 220)),
            ((215, 218),(182, 207),(151, 215)),((120, 267),(89, 295),(56, 281)),
            ((23, 262),(42, 199),(53, 152)),((62, 105),(78, 67),(119, 55))])
    mask = Image.new("L", SIZE)
    ImageDraw.Draw(mask).polygon(points, fill=255)
    image = blank()
    top, bottom = ((54, 61, 92), (19, 23, 42)) if family == "Xbox" else ((91, 101, 133), (38, 48, 75))
    draw = ImageDraw.Draw(image)
    for y in range(SIZE[1]):
        t = y / SIZE[1]
        draw.line((0,y,511,y),fill=tuple(round(a*(1-t)+b*t) for a,b in zip(top,bottom))+(255,))
    image.putalpha(mask)
    draw = ImageDraw.Draw(image)
    draw.line(points+[points[0]], fill=(5,9,23,255),width=4)
    upper=[p for p in points if p[1]<148]
    draw.line(upper,fill=(156,117,211,255),width=2)
    return image


def cap(radius=18, state=0):
    size=radius*2+8
    image=Image.new("RGBA",(size,size));draw=ImageDraw.Draw(image)
    edge=(124,143,190,255) if state==0 else (64,221,255,255)
    draw.ellipse((1,3,size-2,size-1),fill=(4,8,20,255))
    draw.ellipse((2,1,size-3,size-4),fill=edge)
    draw.ellipse((4,3,size-5,size-6),fill=(22,32,53,255) if state!=2 else (39,91,119,255))
    draw.arc((6,5,size-7,size-8),195,290,fill=(149,167,202,255),width=1)
    return image


def draw_family(family, state):
    anatomy_data=anatomy(family)
    layers=[shell(family)]+[blank() for _ in range(len(LAYERS)-1)]
    grip=ImageDraw.Draw(layers[1]);panel=ImageDraw.Draw(layers[2]);wells=ImageDraw.Draw(layers[3]);buttons=ImageDraw.Draw(layers[4]);symbols=ImageDraw.Draw(layers[5]);fx=ImageDraw.Draw(layers[6])
    for side in (0,1):
        polygon=[(63,174),(113,171),(154,220),(108,277),(74,279),(48,246)]
        if side:polygon=[(512-x,y) for x,y in polygon]
        grip.polygon(polygon,fill=(17,23,39,170))
        for y in range(199,270,7):
            for x in range(65,110,7):
                px=512-x if side else x
                if layers[0].getpixel((px,y))[3]:grip.point((px,y),fill=(87,105,141,140))
    panel.line([(109,159),(139,180),(160,220),(205,211),(256,214),(307,211),(352,220),(373,180),(403,159)],fill=(9,15,31,255),width=2)
    if family=="PlayStation":
        panel.rounded_rectangle((198,61,314,117),radius=8,fill=(24,32,52,255),outline=(57,184,243,255),width=2)
        for x in range(210,305,5):
            for y in range(71,108,5):panel.point((x,y),fill=(58,71,100,220))
        panel.polygon([(153,170),(190,150),(229,181),(283,181),(322,150),(359,170),(348,226),(164,226)],fill=(15,22,39,255))
    else:
        panel.ellipse((239,63,273,97),fill=(12,20,36,255),outline=(89,113,159,255),width=2)
        panel.polygon([(247,73),(256,69),(265,73),(256,88)],fill=(255,143,40,255))
    for key in ("left","right"):
        x,y=anatomy_data[key]
        wells.ellipse((x-34,y-34,x+34,y+34),fill=(5,10,21,255),outline=(82,100,137,255),width=3)
        wells.arc((x-30,y-30,x+30,y+30),15,170,fill=(36,54,87,255),width=2)
        buttons.ellipse((x-22,y-22,x+22,y+22),fill=(22,30,46,255),outline=(111,132,168,255),width=2)
        buttons.ellipse((x-14,y-14,x+14,y+14),outline=(7,13,26,255),width=1)
    font_path=ROOT/"Assets/CampusGlitch/Resources/CG/Typography/BarlowCondensed-SemiBold.ttf"
    font=ImageFont.truetype(str(font_path),19) if font_path.exists() else ImageFont.truetype("C:/Windows/Fonts/arialbd.ttf",16)
    identity=ImageDraw.Draw(layers[7]);orange=(255,143,35,255);purple=(165,76,238,255)
    for side in (0,1):
        trace=[(68,177),(56,226),(61,253),(77,269),(95,269)]
        if side:trace=[(512-x,y) for x,y in trace]
        identity.line(trace,fill=orange,width=3)
        for y in (210,220,230):
            x=76 if side==0 else 436;identity.line((x,y,x+(9 if side==0 else -9),y+5),fill=purple,width=2)
    identity.line((186,222,219,218,293,218,326,222),fill=purple,width=2)
    identity.line((205,218,224,215,288,215,307,218),fill=(64,199,243,255),width=1)
    identity.line((180,55,205,57),fill=orange,width=3);identity.line((307,57,332,55),fill=orange,width=3)
    badge_y=79 if family=="Xbox" else 148
    identity.polygon([(247,badge_y-8),(251,badge_y-3),(256,badge_y-6),(261,badge_y-3),(265,badge_y-8),(263,badge_y+4),(256,badge_y+10),(249,badge_y+4)],fill=(16,24,43,255),outline=orange,width=1)
    identity.line((251,badge_y,254,badge_y+2,256,badge_y+7,258,badge_y+2,261,badge_y),fill=orange,width=1)
    for x in (249,254,259):panel.ellipse((x,168,x+2,170),fill=(118,146,184,255))
    cx,cy=anatomy_data["face"]
    for dx,dy,label,color in [(0,-34,"Y",(255,218,52,255)),(35,0,"B",(255,82,102,255)),(0,34,"A",(83,243,155,255)),(-35,0,"X",(66,196,255,255))]:
        x,y=cx+dx,cy+dy
        image=cap(18,state if label=="A" else 0);layers[4].alpha_composite(image,(x-image.width//2,y-image.height//2))
        if family=="Xbox":symbols.text((x,y-1),label,font=font,fill=color,anchor="mm")
        elif label=="Y":symbols.polygon([(x,y-8),(x-8,y+6),(x+8,y+6),(x,y-8)],outline=color,width=2)
        elif label=="B":symbols.ellipse((x-7,y-7,x+7,y+7),outline=color,width=2)
        elif label=="A":symbols.line((x-6,y-6,x+6,y+6),fill=color,width=2);symbols.line((x-6,y+6,x+6,y-6),fill=color,width=2)
        else:symbols.rectangle((x-7,y-7,x+7,y+7),outline=color,width=2)
    dx,dy=anatomy_data["dpad"]
    buttons.polygon([(dx-9,dy-27),(dx+9,dy-27),(dx+9,dy-9),(dx+27,dy-9),(dx+27,dy+9),(dx+9,dy+9),(dx+9,dy+27),(dx-9,dy+27),(dx-9,dy+9),(dx-27,dy+9),(dx-27,dy-9),(dx-9,dy-9)],fill=(13,21,39,255),outline=(109,132,174,255),width=2)
    for key in ("view","menu"):
        x,y=anatomy_data[key];buttons.rounded_rectangle((x-11,y-8,x+11,y+8),radius=5,fill=(13,21,39,255),outline=(125,148,185,255),width=1)
        if key=="menu":
            for offset in (-3,0,3):symbols.line((x-4,y+offset,x+4,y+offset),fill=(211,226,245,255))
        else:symbols.rectangle((x-5,y-3,x+1,y+3),outline=(211,226,245,255));symbols.rectangle((x-1,y-5,x+5,y+1),outline=(211,226,245,255))
    for x,trigger,bumper in [(108,"LT","LB"),(332,"RT","RB")]:
        buttons.rounded_rectangle((x,5,x+80,32),radius=7,fill=(17,25,43,255),outline=(130,150,187,255),width=2)
        buttons.rounded_rectangle((x-6,37,x+86,62),radius=7,fill=(26,38,61,255),outline=(130,150,187,255),width=2)
        symbols.text((x+40,18),trigger if family=="Xbox" else "L2" if x==108 else "R2",font=font,fill=(226,236,255,255),anchor="mm")
        symbols.text((x+40,49),bumper if family=="Xbox" else "L1" if x==108 else "R1",font=font,fill=(226,236,255,255),anchor="mm")
    if state in (1,2):
        x,y=cx,cy+34;fx.ellipse((x-24,y-24,x+24,y+24),outline=(63,225,255,255),width=3 if state==2 else 1)
        fx.line((183,219,329,219),fill=(54,185,255,230),width=2)
    if state==3:
        for index,layer in enumerate(layers):
            alpha=layer.getchannel("A").point(lambda a:a//2)
            grey=layer.convert("L").convert("RGBA");grey.putalpha(alpha);layers[index]=grey
    return layers


def composite(layers):
    image=blank()
    for layer in layers:image.alpha_composite(layer)
    return image


def string(value):
    raw=value.encode("utf-8");return struct.pack("<H",len(raw))+raw


def chunk(kind,payload):
    return struct.pack("<IH",len(payload)+6,kind)+payload


def write_aseprite(path,frames,layer_names=LAYERS,tag_names=None):
    output=[]
    for index,layers in enumerate(frames):
        parts=[]
        if index==0:
            parts += [chunk(0x2004,struct.pack("<6HB3x",3,0,0,0,0,0,255)+string(name)) for name in layer_names]
            names=tag_names or [family+"_"+STATES[i%4] for i,family in enumerate(("Xbox",)*4+("PlayStation",)*4)]
            tags=struct.pack("<H8x",len(names))
            for frame,name in enumerate(names):
                tags+=struct.pack("<HHBH6x3BB",frame,frame,0,0,90,180,255,0)+string(name)
            parts.append(chunk(0x2018,tags))
            parts.append(chunk(0x2007,struct.pack("<HHi8x",1,0,0)))
        for layer,image in enumerate(layers):
            bounds=image.getbbox()
            if not bounds:continue
            x,y,right,bottom=bounds;cropped=image.crop(bounds)
            payload=struct.pack("<HhhBHh5xHH",layer,x,y,255,2,0,cropped.width,cropped.height)+zlib.compress(cropped.tobytes(),9)
            parts.append(chunk(0x2005,payload))
        data=b"".join(parts);output.append(struct.pack("<IHHH2xI",16+len(data),0xF1FA,len(parts),120,len(parts))+data)
    header=bytearray(128)
    struct.pack_into("<IHHHHHIHII",header,0,128+sum(map(len,output)),0xA5E0,len(frames),512,300,32,1,120,0,0)
    struct.pack_into("<HBBhhHH",header,32,0,1,1,0,0,16,16)
    path.write_bytes(bytes(header)+b"".join(output))


def keyboard_keys():
    keys=[]
    def add(binding,label,x,y,w=1):keys.append(dict(binding=binding,label=label,x=18+x*30,y=48+y*37,w=w*30-3,h=32))
    add("Escape","ESC",0,-1)
    for i in range(12):add("F"+str(i+1),"F"+str(i+1),2+i*.93,-1,.9)
    numbers=[("Backquote","`" )]+[("Digit"+str(i%10),str(i%10)) for i in range(1,11)]+[("Minus","-"),("Equals","=")]
    for i,(binding,label) in enumerate(numbers):add(binding,label,i,0)
    add("Backspace","BORRAR",13,0,2.4)
    add("Tab","TAB",0,1,1.5)
    for i,c in enumerate("QWERTYUIOP"):add(c,c,1.5+i,1)
    add("LeftBracket","[",11.5,1);add("RightBracket","]",12.5,1);add("Backslash","\\",13.5,1,1.9)
    add("CapsLock","BLOQ",0,2,1.8)
    for i,c in enumerate("ASDFGHJKL"):add(c,c,1.8+i,2)
    add("Semicolon",";",10.8,2);add("Quote","'",11.8,2);add("Enter","ENTER",12.8,2,2.6)
    add("LeftShift","SHIFT",0,3,2.3)
    for i,c in enumerate("ZXCVBNM"):add(c,c,2.3+i,3)
    for i,(binding,label) in enumerate([("Comma",","),("Period","."),("Slash","/")]):add(binding,label,9.3+i,3)
    add("RightShift","SHIFT",12.3,3,1.1);add("UpArrow","UP",13.4,3)
    add("LeftCtrl","CTRL",0,4,1.4);add("LeftMeta","WIN",1.4,4,1.2);add("LeftAlt","ALT",2.6,4,1.2);add("Space","ESPACIO",3.8,4,5.1);add("RightAlt","ALT",8.9,4,1.2);add("ContextMenu","MENU",10.1,4,1.1);add("RightCtrl","CTRL",11.2,4,1.2)
    add("LeftArrow","LEFT",12.4,4);add("DownArrow","DOWN",13.4,4);add("RightArrow","RIGHT",14.4,4)
    return keys


def keyboard_art():
    keys=keyboard_keys();frames=[];small=ImageFont.truetype("C:/Windows/Fonts/arialbd.ttf",10)
    for state in range(4):
        layers=[blank() for _ in range(5)];body=ImageDraw.Draw(layers[0]);body.rounded_rectangle((4,3,507,286),radius=14,fill=(11,18,35,255),outline=(106,80,155,255),width=2)
        body.line((20,284,490,284),fill=(255,141,35,255),width=3)
        brand=ImageDraw.Draw(layers[1]);brand.text((19,270),"CAMPUS GLITCH  /  CONTROL DE ENTRADA",font=small,fill=(226,152,72,255))
        draw=ImageDraw.Draw(layers[2]);labels=ImageDraw.Draw(layers[3]);effect=ImageDraw.Draw(layers[4])
        for key in keys:
            x,y,w,h=[key[k] for k in ("x","y","w","h")];active=key["binding"] in ("A","D","Space","LeftShift","J","E","Q","Escape")
            draw.rounded_rectangle((x,y,x+w,y+h),radius=3,fill=(6,10,23,255));draw.rounded_rectangle((x,y,x+w,y+h-3),radius=3,fill=(25,36,58,255),outline=(153,98,52,255) if active else (65,80,112,255),width=1)
            labels.text((x+w/2,y+h/2-2),key["label"],font=small,fill=(247,201,128,255) if active else (202,219,240,255),anchor="mm")
            if key["binding"]=="A" and state in (1,2):effect.rounded_rectangle((x-1,y-1,x+w+1,y+h-1),radius=3,outline=(65,222,255,255),width=2 if state==2 else 1)
        if state==3:
            for i,layer in enumerate(layers):a=layer.getchannel("A").point(lambda v:v//2);layers[i]=layer.convert("L").convert("RGBA");layers[i].putalpha(a)
        frames.append(layers)
    composite(frames[0][:2]).save(EXPORT/"Keyboard_Campus.png",optimize=True)
    composite(frames[0]).save(SOURCE/"Keyboard_Guia.png",optimize=True)
    write_aseprite(SOURCE/"CampusKeyboard.aseprite",frames,["01_Carcasa","02_Identidad","03_Teclas","04_Rotulos","05_Estado"],["Teclado_"+s for s in STATES])
    (EXPORT/"KeyboardLayout.json").write_text(json.dumps(dict(keys=keys),indent=2),encoding="utf-8")
    caps=Image.new("RGBA",(96*3,64))
    for state in range(3):
        d=ImageDraw.Draw(caps);x=state*96;edge=(76,94,128,255) if state==0 else (255,149,52,255) if state==1 else (65,220,255,255)
        d.rounded_rectangle((x+1,4,x+94,62),radius=6,fill=(5,9,20,255));d.rounded_rectangle((x+1,1,x+94,57),radius=6,fill=(23,34,57,255) if state!=2 else (30,78,106,255),outline=edge,width=2);d.line((x+8,7,x+86,7),fill=(108,126,160,255),width=1)
    caps.save(EXPORT/"Keyboard_KeyStates.png",optimize=True)


def main():
    SOURCE.mkdir(parents=True,exist_ok=True);EXPORT.mkdir(parents=True,exist_ok=True)
    frames=[]
    for family in ("Xbox","PlayStation"):
        for state in range(4):frames.append(draw_family(family,state))
        neutral=draw_family(family,0)
        composite(neutral[:4]+[neutral[7]]).save(EXPORT/("Controller_"+family+".png"),optimize=True)
        composite(neutral).save(SOURCE/("Controller_"+family+"_Guia.png"),optimize=True)
    write_aseprite(SOURCE/"CampusController.aseprite",frames)
    caps=Image.new("RGBA",(96*3,96))
    for state in range(3):
        image=cap(40,state);caps.alpha_composite(image,(state*96+(96-image.width)//2,(96-image.height)//2))
    caps.save(EXPORT/"Controller_ButtonStates.png",optimize=True)
    contact=Image.new("RGBA",(512*4,300*2),(8,14,30,255))
    for i,frame in enumerate(frames):contact.alpha_composite(composite(frame),((i%4)*512,(i//4)*300))
    contact.save(SOURCE/"Controller_Estados.png",optimize=True)
    (SOURCE/"Controller_Anatomia.json").write_text(json.dumps({f:anatomy(f) for f in ("Xbox","PlayStation")},indent=2),encoding="utf-8")
    keyboard_art()
    print("Aseprite: 512x300, 8 capas, 8 frames, RGBA, cels zlib; exportados 2 cuerpos y 3 estados de boton.")


if __name__=="__main__":main()
