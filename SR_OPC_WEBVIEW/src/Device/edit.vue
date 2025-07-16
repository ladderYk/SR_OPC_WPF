<template>
    <div class="agv_main">
        <el-page-header class="page_header" @back="goBack">
            <template #content>
                <span class="text-large font-600 mr-3"> {{ form.id ? "编辑" : "添加" }}设备 </span>
            </template>
        </el-page-header>
        <el-form :model="form" :inline="true" label-position="top" ref="formRef" label-width="auto">
            <el-form-item prop="name" label="名称" :rules="[
                { required: true, message: '名称不能为空' }
            ]">
                <el-input v-model="form.name" autocomplete="off" />
            </el-form-item>
            <el-form-item prop="model" label="类型" :rules="[
                { required: true, message: '类型不能为空' }
            ]">
                <el-select-v2 style="width: 200px" :props="selProps" :options="options" v-model="form.model"
                    value-key="Name" />
            </el-form-item>
            <el-form-item prop="ip" label="IP" :rules="[
                { required: true, message: 'IP不能为空' }
            ]">
                <el-input v-model="form.ip" autocomplete="off" />
            </el-form-item>
            <el-form-item prop="port" label="端口号" :rules="[
                { required: true, message: '端口号不能为空' }
            ]">
                <el-input v-model="form.port" autocomplete="off" />
            </el-form-item>
            <el-form-item prop="timeout" label="连接超时时间" :rules="[
                { required: true, message: '连接超时时间不能为空' }
            ]">
                <el-input v-model="form.timeout" autocomplete="off" />
            </el-form-item>
            <el-form-item prop="cycle" label="循环周期" :rules="[
                { required: true, message: '循环周期不能为空' }
            ]">
                <el-input v-model="form.cycle" autocomplete="off" />
            </el-form-item>
        </el-form>
        <div>
            <el-button type="primary" @click="submitForm(formRef)">
                提交
            </el-button>
            <el-button @click="resetForm()">重置</el-button>
        </div>
    </div>
</template>
<script setup>
import { onMounted, ref, reactive } from "vue";

const props = defineProps(['form'])
const selProps = {
    label: 'Name',
    value: 'Name',
}
var form = props.form;
const options = ref([]);
const formRef = ref();
onMounted(() => {
    // get("/DeviceList").then(data => {
    //     tableData.value = data;
    // });
    fetch("/dType.json").then(v => v.json()).then(data => {
        options.value = data;
    });
    // get("/TypeList").then(data => {
    //     options.value = data;
    // });

});
</script>