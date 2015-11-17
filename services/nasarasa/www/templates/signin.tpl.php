{extends 'page.tpl.php'}

{block 'title'}Sign in{/block}

{block 'content'}
    <h3>Sign in</h3>

    <form method="POST" action="">
        <div class="form-group">
            <label>Login</label>
            <input type="text" class="form-control" placeholder="Login" name="login" />
        </div>
        <div class="form-group">
            <label>Password</label>
            <input type="password" class="form-control" placeholder="Password" name="password" />
        </div>
        <button type="submit" class="btn btn-default">Sign in</button>
            {if $result}
                <div class="alert alert-danger">
                    {$result}
                </div>
            {/if}
   </form>
{/block}