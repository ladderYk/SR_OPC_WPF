<template>
    <el-tabs>
        <el-tab-pane label="原始数据">
            <el-table :data="initData">
                <el-table-column label="名称" prop="Name" />
                <el-table-column label="标签" prop="Tag" />
                <el-table-column label="数据" prop="Data" />
            </el-table>
        </el-tab-pane>
        <el-tab-pane label="解析数据">
            <!-- <el-table :data="cfgData">
                <el-table-column type="index" />
                <el-table-column label="值">
                    <template #default="scope">
                        {{ scope.row }}
                    </template>
</el-table-column>
</el-table> -->
            <JsonFormat v-model="cfgData" />
        </el-tab-pane>
    </el-tabs>
</template>
<script setup>
import { onMounted, ref, onUnmounted, watch, watchEffect } from "vue";
import JsonFormat from '../components/JsonFormat.vue';
import { getDeviceData } from "../utils/dotnet";

let sokect = null;

const props = defineProps(['form'])

const form = props.form;
var name;
const options = ref([]);
const initData = ref([]);
const cfgData = ref(null);
watch(form, () => {
    getDeviceData(form.Name).then(data => {
        var oData = data[0];
        var nData = data[1];
        initData.value = oData;
        cfgData.value = nData;
    });
    sokect.send(JSON.stringify({ topic: 'data/' + name, action: 'unsubscribe' }));
    initData.value = [];
    cfgData.value = [];
    sokect?.close(1000);
    sokect = new WebSocket(ws);
    sokect.onopen = () => {
        sokect.send(JSON.stringify({ topic: 'data/' + form.Name, action: 'subscribe' }));
    };
    sokect.onmessage = (message) => {
        var data = JSON.parse(message.data);
        var oData = data[0];
        var nData = data[1];
        initData.value = oData;
        cfgData.value = nData;
        //textarea.value.unshift(message.data);
    };

    name = form.Name;
});
onMounted(() => {
    // get("/DeviceList").then(data => {
    //     tableData.value = data;
    // });
    // fetch("/dType.json").then(v => v.json()).then(data => {
    //     options.value = data;
    // });
    getDeviceData(form.Name).then(data => {
        var oData = data[0];
        var nData = data[1];
        initData.value = oData;
        cfgData.value = nData;
    });
    sokect = new WebSocket(ws);
    name = form.Name;
    sokect.onopen = () => {
        sokect.send(JSON.stringify({ topic: 'data/' + name, action: 'subscribe' }));
    };
    sokect.onerror = () => {
        // setTimeout(() => {
        //   document.location.reload();
        // }, 10000);
    }
    sokect.onmessage = (message) => {
        var data = JSON.parse(message.data);
        var oData = data[0];
        var nData = data[1];
        initData.value = oData;
        cfgData.value = nData;
        //textarea.value.unshift(message.data);
    };
    // get("/TypeList").then(data => {
    //     options.value = data;
    // });

});

onUnmounted(() => {
    sokect.send(JSON.stringify({ topic: 'data/' + form.Name, action: 'unsubscribe' }));
    sokect.close(1000);
})
</script>