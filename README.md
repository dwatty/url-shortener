# URL Shortener

David Fowler retweeted a video of someone making a URL shortener and I thought I'd follow along with that to make my first .NET 7 hobby project.  It also gave me a chance to try the minimal APIs.

Link to tsjdevapps's video: https://www.youtube.com/watch?v=sVHkeyCyucw


## Random Dev Notes

Ran into target-typed new expressions as part of this little project.  It's neat, it doesn't come naturally to me yet.

This:
``` 
Hashids _hashIds = new("URL Shortern", 5); 
```

Is Equivalent to this:
``` 
var varHashes = new Hashids("URL Shortener", 5); 
```
