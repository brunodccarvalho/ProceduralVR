using System.Collections.Generic;

public struct Highlight {
    public bool hovered;
    public bool exposed;
    public bool selected;
    public bool grabbed;
    public bool slaveGrabbed;
    public int color;

    public static Highlight operator +(Highlight a, Highlight b) {
        var c = new Highlight();
        c.hovered = a.hovered | b.hovered;
        c.exposed = a.exposed | b.exposed;
        c.selected = a.selected | b.selected;
        c.grabbed = a.grabbed | b.grabbed;
        c.slaveGrabbed = a.slaveGrabbed | b.slaveGrabbed;
        c.color = a.color >= b.color ? a.color : b.color;
        return c;
    }

    public string GetVariant() {
        if (grabbed) return "Grabbed";
        if (slaveGrabbed) return "SlaveGrabbed";
        if (hovered && selected) return "HoveredSelected";
        if (hovered) return "Hovered"; // amarelo
        if (selected) return "Selected"; // azul
        if (color > 0) return string.Format("Color-{0}", color);
        // if (exposed) return "Exposed";
        return "Default";
    }
}
