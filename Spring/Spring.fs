﻿#light

namespace Spring

 module Game = 
  open System
  open FReactive.Misc
  open FReactive.FReactive
  open FReactive.Integration
  open FReactive.Lib
  open Common.Random
  open Xna.Main

  open Microsoft.Xna.Framework
  open Microsoft.Xna.Framework.Graphics
  open Microsoft.Xna.Framework.Input
  

  let (.*) a b = (pureB (*)) <.> a <.> b 
  let (.-) a b = (pureB (-)) <.> a <.> b 
  let (.+) a b = (pureB (+)) <.> a <.> b 
  let cosB = pureB Math.Cos
  let sinB = pureB Math.Sin
  let (.<=) a b = pureB (<=) <.> a <.> b
  let (.>=) a b = pureB (>=) <.> a <.> b
  
  let mainGame (game:Game) = 
        let condxf = pureB (fun x -> x <= (-1.0) || x >= 1.0)
       // let condyf = pureB (fun y -> y <= (-1.0) || y >= 1.0)
        let sidef f = pureB (fun x -> f x
                                      x)

        let mousePos = mousePos game
        let rec mousePosEvt = Evt (fun _ -> (mousePos(), fun() -> mousePosEvt))
        let mousePosB  = stepB (0.0, 0.0) mousePosEvt
        let mousePosXB = pureB fst <.> mousePosB
        let mousePosYB = pureB snd <.> mousePosB

        let mkVelocity t0 v0 accB hitE =
            let rec proc t0 v0 e0 = 
                let v0' = e0 t0 v0
                let v = integrate accB t0 v0'
                let fE = hitE --> (fun _ v -> -v)
                Disc (v, fE, proc)
            discontinuityE (proc t0 v0 (fun _ v -> -v))

        let rec sys t0 x0 vx0 mousePosXB = 
            let x' = aliasB x0
            let vx' = aliasB x0
            let hitE = whenE (condxf <.> (fst x'))
            let accxB =  pureB 1.0 .* (mousePosXB .- (fst x')) .- (pureB 0.05 .* (fst vx')) 
            let vx = bindAliasB (mkVelocity t0 vx0 accxB hitE) vx'
                            
            let x =  (bindAliasB (integrate vx t0 x0 ) x') 
            x 
        coupleB() <.> (sys 0.0 0.5 0.0  mousePosXB) <.> (sys 0.0 0.5 0.0 mousePosYB) // |>  tronB "x=" 


   (*     
    let mkMovement t0 x0 velocityB =
        let integrate = integrateGenB vectorNumClass
        let rec proc t0 x0 e0 = 
            let x0' = e0 t0 x0
            let x = integrate velocityB t0 x0'
            let boxE = (whileE ((pureB (inBoxPred >> not)) <.> x)) --> (fun _ x -> adaptToBox x)
            Disc (x, boxE, proc)
        discontinuityE (proc t0 x0 (fun _ x -> adaptToBox x))

 type Discontinuity<'a, 'b> = Disc of ('a Behavior *  (Time -> 'a -> 'b) Event * (Time -> 'a -> (Time -> 'a -> 'b) ->  Discontinuity<'a, 'b>))
    
 let rec discontinuityE (Disc (xB, predE, bg))  = 
        let evt = snapshotE predE ((coupleB() <.> timeB <.> xB))  =>>  
                        (fun (e,(t,vb)) -> let disc = bg t vb e
                                           discontinuityE disc)
        untilB xB evt

        *)
  let renderer (x, y) (gd:GraphicsDevice) = 
        let n_verts = 2
        let random_vert _ = Graphics.VertexPositionColor(Vector3(0.f, 0.f, 0.f), Graphics.Color.White)
        let vertex = Array.init n_verts random_vert
        vertex.[1].Position <- Vector3((float32)x, (float32)y, (float32) 0.0)
        gd.DrawUserPrimitives(PrimitiveType.LineList, vertex, 0, n_verts/2)


  let renderedGame (game:Game) = 
        let stateB = mainGame game
        (pureB renderer) <.> stateB 

  do use game = new XnaTest2(renderedGame)
     game.Run() 