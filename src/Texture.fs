namespace ThreesAI
#nowarn "9"

open System
open FSharp.NativeInterop
open ResultBuilder
open Rendering
open type SDL2.SDL
open type SDL2.SDL_image

type Texture =
    { Handle: nativeint
      Width: int
      Height: int
      Scale: int }

type ClipRectangle =
    { X: int
      Y: int
      W: int
      H: int }

module Texture =
    let private loadImage path =
        let surface = IMG_Load path
        if surface = IntPtr.Zero then
            Error <| SDL_GetError ()
        else
            Ok surface
            
    let private createTexture renderer surface =
        let texture = SDL_CreateTextureFromSurface (renderer, surface)
        if texture = IntPtr.Zero then
            Error <| SDL_GetError ()
        else
            Ok texture
    
    let create renderer path scale = resultBuilder {
        let! surfaceRef = loadImage path
        let! texture = createTexture renderer surfaceRef
        
        // We have to dereference the surface to access its width and height
        // Convert nativeint to nativeptr and read
        
        // lol pointers in functional programming
        let surfacePtr = surfaceRef |> NativePtr.ofNativeInt
        let surface = NativePtr.read<SDL_Surface> surfacePtr
        let w, h = (surface.w, surface.h)
        
        // Delete surface now that it is loaded as a texture
        SDL_FreeSurface surfaceRef
        
        return { Handle = texture; Width = w; Height = h; Scale = scale }
    }

    let render texture (renderer: Renderer) ((x, y): Coords) (clipRectangle: ClipRectangle option) =
        match clipRectangle with
        | None ->
            let mutable destRect = SDL_Rect ( x = x, y = y, w = texture.Width * texture.Scale, h = texture.Height * texture.Scale)
            
            // What is error handling :S
            SDL_RenderCopy (renderer, texture.Handle, IntPtr.Zero, &destRect) |> ignore
        | Some rect ->
            let mutable sdlRect = SDL_Rect ( x = rect.X, y = rect.Y, w = rect.W, h = rect.H )
            let mutable destRect = SDL_Rect ( x = x, y = y, w = rect.W * texture.Scale, h = rect.H * texture.Scale)
            SDL_RenderCopy (renderer, texture.Handle, &sdlRect, &destRect) |> ignore