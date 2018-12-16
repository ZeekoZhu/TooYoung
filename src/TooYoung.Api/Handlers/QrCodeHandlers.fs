module TooYoung.Api.Handlers.QrCodeHandlers
open System
open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2
open QRCoder

let qrcode (content: string): HttpHandler =
    fun next ctx ->
        use qrCoder = new QRCodeGenerator()
        use codeData = qrCoder.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q)
        use qrCode = new PngByteQRCode(codeData)
        let bytes = qrCode.GetGraphic(20)
        ctx.SetContentType("image/png")
        ctx.WriteBytesAsync(bytes)
        
        
        
let routes: HttpHandler =
    subRouteCi "/qrcode"
        ( choose
            [ GET >=> routeCif "/%s" qrcode
            ]
        )
