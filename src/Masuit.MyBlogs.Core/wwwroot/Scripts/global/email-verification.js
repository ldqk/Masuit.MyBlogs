function forgetEmailVerification() {
  axios.post('/validate/forgetemail', null, {
    headers: {
      RequestVerificationToken: document.querySelector('input[name="__RequestVerificationToken"]').value
    }
  }).then(response => {
    if (response.data && response.data.Success) {
      window.location.reload();
    } else {
      message.error(response.data?.Message || '更换邮箱失败，请重试！');
    }
  }).catch(() => message.error('更换邮箱失败，请稍后重试！'));
}
