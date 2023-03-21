using global::Verse;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssortedTweaks
{
    public class Graphic_MultiAppearances : Graphic
    {
        //protected Graphic[] subGraphics;

        //public override Material MatSingle => this.subGraphics[(int)StuffAppearanceDefOf.Smooth.index].MatSingle;

        private ThingDef StuffOfThing(Thing thing) => thing is IConstructible constructible ? constructible.EntityToBuildStuff() : thing.Stuff;

        public override Material MatAt(Rot4 rot, Thing thing = null) => this.SubGraphicFor(thing,rot).MatAt(rot, thing);

        //multi variables

        private Material[,] mats = new Material[4,(int)StuffAppearanceDefOf.Smooth.index];
        private bool westFlipped;
        private bool eastFlipped;
        private float drawRotatedExtraAngleOffset;
        public const string NorthSuffix = "_north";
        public const string SouthSuffix = "_south";
        public const string EastSuffix = "_east";
        public const string WestSuffix = "_west";

        public string GraphicPath => this.path;

        public override Material MatSingle => this.subGraphics[1,(int)StuffAppearanceDefOf.Smooth.index].MatSingle;

        public Graphic[,] subGraphics;

        public override bool WestFlipped => this.westFlipped;

        public override bool EastFlipped => this.eastFlipped;

        public override bool ShouldDrawRotated
        {
          get
          {
            if (this.data != null && !this.data.drawRotated)
              return false;
            return this.MatEast == this.MatNorth || this.MatWest == this.MatNorth;
          }
        }

        public override float DrawRotatedExtraAngleOffset => this.drawRotatedExtraAngleOffset;

        public override void Init(GraphicRequest req)
        {
            this.data = req.graphicData;
            this.path = req.path;
            this.color = req.color;
            this.drawSize = req.drawSize;
            List<StuffAppearanceDef> defsListForReading = DefDatabase<StuffAppearanceDef>.AllDefsListForReading;
            this.subGraphics = new Graphic[4,defsListForReading.Count];
            for (int x = 0; x < this.subGraphics.GetLength(0); ++x)
            {
                for (int y = 0; y < this.subGraphics.GetLength(1); y++)
                {
                    StuffAppearanceDef stuffAppearance = defsListForReading[y];
                    string folderPath = req.path;
                    if (!stuffAppearance.pathPrefix.NullOrEmpty())
                        folderPath = stuffAppearance.pathPrefix + "/" + ((IEnumerable<string>)folderPath.Split('/')).Last<string>();
                    Texture2D texture2D = ContentFinder<Texture2D>.GetAllInFolder(folderPath).Where<Texture2D>((Func<Texture2D, bool>)(t => t.name.EndsWith(stuffAppearance.defName))).FirstOrDefault<Texture2D>();
                    if ((UnityEngine.Object)texture2D != (UnityEngine.Object)null)
                        this.subGraphics[x,y] = GraphicDatabase.Get<Graphic_Single>(folderPath + "/" + texture2D.name, req.shader, this.drawSize, this.color);
                    InitMulti(subGraphics[x, y], req, y);
                }
 
                for (int a = 0; a < this.subGraphics.GetLength(0); ++a)
                {
                    for (int b = 0; b < this.subGraphics.GetLength(1); ++b)
                    {
                        if (this.subGraphics[a,b] == null)
                            this.subGraphics[a,b] = this.subGraphics[a,(int)StuffAppearanceDefOf.Smooth.index];
                    }
                }
            }
        }

        public void InitMulti(Graphic graphic, GraphicRequest req, int index)
        {
            //this.data = req.graphicData;
            graphic.path = req.path;
            graphic.maskPath = req.maskPath;
            graphic.color = req.color;
            graphic.colorTwo = req.colorTwo;
            graphic.drawSize = req.drawSize;
            Texture2D[,] texture2DArray1 = new Texture2D[mats.GetLength(0), mats.GetLength(1)];
            texture2DArray1[0, index] = ContentFinder<Texture2D>.Get(req.path + "_north", false);
            texture2DArray1[1, index] = ContentFinder<Texture2D>.Get(req.path + "_east", false);
            texture2DArray1[2, index] = ContentFinder<Texture2D>.Get(req.path + "_south", false);
            texture2DArray1[3, index] = ContentFinder<Texture2D>.Get(req.path + "_west", false);
            if ((Object)texture2DArray1[0, index] == (Object)null)
            {
                if ((Object)texture2DArray1[2, index] != (Object)null)
                {
                    texture2DArray1[0, index] = texture2DArray1[2, index];
                    this.drawRotatedExtraAngleOffset = 180f;
                }
                else if ((Object)texture2DArray1[1, index] != (Object)null)
                {
                    texture2DArray1[0, index] = texture2DArray1[1, index];
                    this.drawRotatedExtraAngleOffset = -90f;
                }
                else if ((Object)texture2DArray1[3, index] != (Object)null)
                {
                    texture2DArray1[0, index] = texture2DArray1[3, index];
                    this.drawRotatedExtraAngleOffset = 90f;
                }
                else
                    texture2DArray1[0, index] = ContentFinder<Texture2D>.Get(req.path, false);
            }
            if ((Object)texture2DArray1[0, index] == (Object)null)
            {
                Log.Error("Failed to find any textures at " + req.path + " while constructing " + this.ToStringSafe<Graphic_MultiAppearances>());
            }
            else
            {
                if ((Object)texture2DArray1[2, index] == (Object)null)
                    texture2DArray1[2, index] = texture2DArray1[0, index];
                if ((Object)texture2DArray1[1, index] == (Object)null)
                {
                    if ((Object)texture2DArray1[3, index] != (Object)null)
                    {
                        texture2DArray1[1, index] = texture2DArray1[3, index];
                        this.eastFlipped = this.DataAllowsFlip;
                    }
                    else
                        texture2DArray1[1, index] = texture2DArray1[0, index];
                }
                if ((Object)texture2DArray1[3, index] == (Object)null)
                {
                    if ((Object)texture2DArray1[1, index] != (Object)null)
                    {
                        texture2DArray1[3, index] = texture2DArray1[1, index];
                        this.westFlipped = this.DataAllowsFlip;
                    }
                    else
                        texture2DArray1[3, index] = texture2DArray1[0, index];
                }
                Texture2D[,] texture2DArray2 = new Texture2D[mats.GetLength(0),mats.GetLength(1)];
                if (req.shader.SupportsMaskTex())
                {
                    string str1 = this.maskPath.NullOrEmpty() ? this.path : this.maskPath;
                    string str2 = this.maskPath.NullOrEmpty() ? "m" : string.Empty;
                    texture2DArray2[0, index] = ContentFinder<Texture2D>.Get(str1 + "_north" + str2, false);
                    texture2DArray2[1, index] = ContentFinder<Texture2D>.Get(str1 + "_east" + str2, false);
                    texture2DArray2[2, index] = ContentFinder<Texture2D>.Get(str1 + "_south" + str2, false);
                    texture2DArray2[3, index] = ContentFinder<Texture2D>.Get(str1 + "_west" + str2, false);
                    if ((Object)texture2DArray2[0, index] == (Object)null)
                    {
                        if ((Object)texture2DArray2[2, index] != (Object)null)
                            texture2DArray2[0, index] = texture2DArray2[2, index];
                        else if ((Object)texture2DArray2[1, index] != (Object)null)
                            texture2DArray2[0, index] = texture2DArray2[1, index];
                        else if ((Object)texture2DArray2[3, index] != (Object)null)
                            texture2DArray2[0, index] = texture2DArray2[3, index];
                    }
                    if ((Object)texture2DArray2[2, index] == (Object)null)
                        texture2DArray2[2, index] = texture2DArray2[0, index];
                    if ((Object)texture2DArray2[1, index] == (Object)null)
                        texture2DArray2[1, index] = !((Object)texture2DArray2[3, index] != (Object)null) ? texture2DArray2[0, index] : texture2DArray2[3, index];
                    if ((Object)texture2DArray2[3, index] == (Object)null)
                        texture2DArray2[3, index] = !((Object)texture2DArray2[1, index] != (Object)null) ? texture2DArray2[0, index] : texture2DArray2[1, index];
                }
                for (int x = 0; x < mats.GetLength(0); ++x)
                {
                    for (int y = 0; y < mats.GetLength(1); y++)
                    {
                        this.mats[x,y] = MaterialPool.MatFrom(new MaterialRequest()
                        {
                            mainTex = (Texture)texture2DArray1[x,y],
                            shader = req.shader,
                            color = this.color,
                            colorTwo = this.colorTwo,
                            maskTex = texture2DArray2[x,y],
                            shaderParameters = req.shaderParameters,
                            renderQueue = req.renderQueue
                        });
                    }
                }

            }
        }

        public override Graphic GetColoredVersion(
            Shader newShader,
            Color newColor,
            Color newColorTwo)
        {
            if (newColorTwo != Color.white)
                Log.ErrorOnce("Cannot use Graphic_Appearances.GetColoredVersion with a non-white colorTwo.", 9910251);
            return GraphicDatabase.Get<Graphic_MultiAppearances>(this.path, newShader, this.drawSize, newColor, Color.white, this.data);
        }

        public override Material MatSingleFor(Thing thing) => this.SubGraphicFor(thing,thing.Rotation).MatSingleFor(thing);

        public override void DrawWorker(
            Vector3 loc,
            Rot4 rot,
            ThingDef thingDef,
            Thing thing,
            float extraRotation)
        {
            this.SubGraphicFor(thing, rot).DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public Graphic SubGraphicFor(Thing thing, Rot4 rot)
        {
            StuffAppearanceDef smooth = StuffAppearanceDefOf.Smooth;
            return thing != null ? this.SubGraphicFor(this.StuffOfThing(thing), rot) : this.subGraphics[rot.AsInt,(int)smooth.index];
        }

        public Graphic SubGraphicFor(ThingDef stuff, Rot4 rot)
        {
            StuffAppearanceDef app = StuffAppearanceDefOf.Smooth;
            if (stuff != null && stuff.stuffProps.appearance != null)
                app = stuff.stuffProps.appearance;
            return this.SubGraphicFor(app,rot);
        }

        public Graphic SubGraphicFor(StuffAppearanceDef app, Rot4 rot) => this.subGraphics[rot.AsInt,(int)app.index];

        public override string ToString() => "Appearance(path=" + this.path + ", color=" + (object)this.color + ", colorTwo=" + (object)this.colorTwo + ")";

        public override int GetHashCode() => Gen.HashCombineStruct<Color>(Gen.HashCombineStruct<Color>(Gen.HashCombine<string>(0, this.path), this.color), this.colorTwo);
    }

}
