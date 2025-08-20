<template>
  <pre v-html="preSetColor(data)" id="preDom"></pre>
</template>
 
<script setup>
const data = defineModel()
const props = defineProps({
  // 文字颜色
  colors: {
    type: Object,
    default: () => ({}),
  },
  // 展示json的时候，步长
  step: {
    type: [Number, String],
    default: 4,
  },
})
// 颜色映射关系
const colorMap = {
  string: 'green',
  number: 'darkorange',
  boolean: 'blue',
  null: 'magenta',
  key: 'red',
}
// pre设置颜色
function preSetColor(data) {
  const obj = {
    ...colorMap,
    ...props.colors,
  }
  const stringSty = `color: ${obj['string']}`;
  const numberSty = `color: ${obj['number']}`;
  const booleanSty = `color: ${obj['boolean']}`;
  const nullSty = `color: ${obj['null']}`;
  const keyStry = `color: ${obj['key']}`;
  function syntaxHighlight(json) {
    if (typeof json != 'string') {
      json = JSON.stringify(json, undefined, 2)
    }
    json = json.replace(/&/g, '&').replace(/</g, '<').replace(/>/g, '>')
    return json.replace(
      /("(\\u[a-zA-Z0-9]{4}|\\[^u]|[^\\"])*"(\s*:)?|\b(true|false|null)\b|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?)/g,
      function (match) {
        var cls = numberSty
        if (/^"/.test(match)) {
          if (/:$/.test(match)) {
            cls = keyStry
          } else {
            cls = stringSty
          }
        } else if (/true|false/.test(match)) {
          cls = booleanSty
        } else if (/null/.test(match)) {
          cls = nullSty
        }
        return '<span style="' + cls + '">' + match + '</span>'
      },
    )
  }
  const handler = JSON.stringify(data, null, +props.step)
  const light = syntaxHighlight(handler)
  return light;
}
</script>