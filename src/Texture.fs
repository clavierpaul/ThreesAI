namespace ThreesAI

open System
open FSharp.NativeInterop
open ResultBuilder
open type SDL2.SDL
open type SDL2.SDL_image

type Texture =
    { Handle: nativeint
      Width: int
      Height: int }

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
    
    let create renderer path = resultBuilder {
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
        
        return { Handle = texture; Width = w; Height = h; }
    }
