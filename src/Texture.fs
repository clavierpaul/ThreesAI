namespace ThreesAI

open ThreesAI

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

type RenderOptions =
    { Renderer: Renderer
      Texture: Texture
      Source: SDL_Rect option
      Dest: SDL_Rect }

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
    
    let renderer texture renderer =
        let w = texture.Width  * texture.Scale * renderScale
        let h = texture.Height * texture.Scale * renderScale
        { Texture = texture
          Renderer = renderer
          Source = None
          Dest = SDL_Rect ( x = 0, y = 0, w = w, h = h ) }
    
    let renderAt (x, y) options =
        { options with Dest = SDL_Rect ( x = x * renderScale * options.Texture.Scale,
                                         y = y * renderScale * options.Texture.Scale,
                                         w = options.Dest.w,
                                         h = options.Dest.h ) }

    let clip (clipRectangle: SDL_Rect) options =
        let w = clipRectangle.w * options.Texture.Scale * renderScale
        let h = clipRectangle.h * options.Texture.Scale * renderScale
        let xDest = options.Dest.x
        let yDest = options.Dest.y
        { options with
            Source = Some clipRectangle
            Dest = SDL_Rect ( x = xDest, y = yDest, w = w, h = h ) }
    
    let render options =
        let mutable dest = options.Dest
        match options.Source with
        | None ->
            // Error handling is for nerds
            SDL_RenderCopy (options.Renderer, options.Texture.Handle, IntPtr.Zero, &dest) |> ignore
        | Some rect ->
            let mutable src = rect
            SDL_RenderCopy (options.Renderer, options.Texture.Handle, &src, &dest) |> ignore