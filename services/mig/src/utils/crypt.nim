import os, strutils, sequtils, ../lib/random, ../lib/nimAES, base64

#type TLock = ref object
#var lock {.global.} = new TLock

proc rnd*(len: Natural): string =
    let buf = random.urandom(len)
    assert len == buf.len
    join(buf.map(proc(b: uint8): string = $chr(b)))

const IVLen = 16
const KeyLen = 32
const BlockSize = 16
const MinEncryptedLen = IVLen + BlockSize

proc genIV(): string =
    rnd(IVLen)

proc genOrReadKey(): string =
    let appDir = getAppDir()
    let filepath = joinPath(appDir, "key")
    try:
        result = readFile(filepath)
        if result.len != KeyLen:
            raise
    except:
        result = rnd(KeyLen)
        writeFile(filepath, result)

let Key* {.global.} = genOrReadKey()

const ZeroPad = '\0'
const OnePad = '\255'

proc pad(data: string): string =
    let padlen = BlockSize - (data.len mod BlockSize)
    repeat(ZeroPad, padlen - 1) & OnePad & data

proc unpad(data: string): string =
    data.substr(data.find(OnePad) + 1)

proc encrypt(key, data: string): string {.gcsafe.} =
    var aes = initAES()
    doAssert aes.setEncodeKey(key)
    let iv = genIV()
    encode(iv.substr() & aes.encryptCBC(iv, pad(data)), 0x7fffffff)

proc decrypt(key, data: string): string {.gcsafe.} =
    let raw = decode(data)
    doAssert raw.len >= MinEncryptedLen
    var aes = initAES()
    doAssert aes.setDecodeKey(key)
    let plain = aes.decryptCBC(raw.substr(0, IVLen - 1), raw.substr(IVLen))
    doAssert(not isNil(plain))
    unpad(plain)

proc encrypt*(data: string): string =
    encrypt(Key, data)

proc decrypt*(data: string): string =
    decrypt(Key, data)

when isMainModule:
    assert Key.len == KeyLen
    assert genIV().len == IVLen
    assert genIV() != genIV()

    echo "    Key: ", encode(Key)
    echo "     IV: ", encode(genIV())

    assert "" == unpad(pad(""))
    assert "qwer" == unpad(pad("qwer"))
    assert "0123456789ABCDEF" == unpad(pad("0123456789ABCDEF"))

    assert "" == decrypt(encrypt(""))
    assert "qwer" == decrypt(encrypt("qwer"))
    assert "0123456789ABCDEF" == decrypt(encrypt("0123456789ABCDEF"))

    assert isNil(try: decrypt("WTF") except AssertionError: nil)
    assert isNil(try: decrypt("0123456789ABCDEF0123456789ABCDEF") except AssertionError: nil)

    import threadpool

    const Iterations = 100

    var a: array[0..Iterations, string]
    var b: array[0..Iterations, FlowVar[string]]

    for i in 0..Iterations:
        let plain = rnd(32)
        a[i] = plain
        b[i] = spawn decrypt(Key, encrypt(Key, plain))

    for i in 0..Iterations:
        assert a[i] == (^b[i])

    echo "[crypt] OK: compiled ", CompileDate, " ", CompileTime